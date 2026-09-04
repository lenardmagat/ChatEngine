using System.Transactions;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using ChatSystem.SystemEvents.Chats;
using ChatSystem.SystemEvents.UnifiedChat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.OfferingMechanism;

public class SaleProposedHandler : IProposedOfferStrategy
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    private readonly ILogger<SaleProposedHandler> _logger;

    public SaleProposedHandler(DbManager db, IHasher hasher, IMediator mediator, ILogger<SaleProposedHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }

    public OfferTye Target => OfferTye.Sale;
    private static readonly List<SaleOfferStatus> NotAllowedStatus = new()
    {
        SaleOfferStatus.Proposed,
        SaleOfferStatus.Countered,
        SaleOfferStatus.Accepted
    };

    public async Task<Result<MessageResponseDTO>> ProposedStrategy(int UserId, ProposedItemDTO proposedItem, CancellationToken cancellation)
    {
        if (proposedItem.SalePayload is null)
        {
            return Result<MessageResponseDTO>.Failure("Invalid request: Missing data", StatusCodes.Status400BadRequest);
        }
        if (proposedItem.SalePayload.QuantityRequested <= 0)
        {
            return Result<MessageResponseDTO>.Failure("Quantity requested must be greater than zero.", StatusCodes.Status400BadRequest);
        }
        if (proposedItem.SalePayload.ProposedPricePerunit < 0)
        {
            return Result<MessageResponseDTO>.Failure("Proposed price per unit cannot be negative.", StatusCodes.Status400BadRequest);
        }

        var decoded = _hasher.DecodeOrFail(proposedItem.ItemId, HashContext.Product);
        if (!decoded.IsSuccess)
        {
            return Result<MessageResponseDTO>.Failure(decoded.Error!, decoded.StatusCode);
        }
        int itemId = decoded.Value;

        var isAlreadyHasOffer = await _db.SaleOffers
            .AsNoTracking()
            .Where(i =>  
                i.RespondedAt == null &&
                i.ItemId == itemId &&
                NotAllowedStatus.Contains(i.Status) &&
                i.Room.Participants.Any(p => p.UserId == UserId))
            .FirstOrDefaultAsync(cancellation);

        if (isAlreadyHasOffer is not null)
        {
            return Result<MessageResponseDTO>.Failure("You already have an ongoing transaction for this item.", StatusCodes.Status400BadRequest);
        }

        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == itemId)
            .Select(p => new { p.Id, p.OwnerUserId, p.IsActive, p.IsAvailable, p.ProductAvailable })
            .FirstOrDefaultAsync(cancellation);

        if (product is null)
        {
            return Result<MessageResponseDTO>.Failure("The item does not exist.", StatusCodes.Status404NotFound);
        }
        if (!product.IsActive || !product.IsAvailable)
        {
            return Result<MessageResponseDTO>.Failure("This product is currently inactive or unavailable.", StatusCodes.Status400BadRequest);
        }
        if (product.OwnerUserId == UserId)
        {
            return Result<MessageResponseDTO>.Failure("You cannot make an offer on your own product.", StatusCodes.Status400BadRequest);
        }

        int productOwnerId = product.OwnerUserId;

        using var transaction = await _db.Database.BeginTransactionAsync(cancellation);
        try
        {
            int affectedRow = await _db.Products
                .Where(p => p.Id == itemId && p.ProductAvailable >= proposedItem.SalePayload.QuantityRequested)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.ProductAvailable, p => p.ProductAvailable - proposedItem.SalePayload.QuantityRequested)
                    .SetProperty(p => p.ReservedProdcut, p => p.ReservedProdcut + proposedItem.SalePayload.QuantityRequested),
                    cancellation
                );

            if (affectedRow == 0)
            {
                return Result<MessageResponseDTO>.Failure("The item does not have enough stock for this request.", StatusCodes.Status400BadRequest);
            }

            GetRoomDataCommand command = new GetRoomDataCommand(UserId, _hasher.CreateHashids(productOwnerId, HashContext.User), null);
            var result = await _mediator.Send(command, cancellation);
            if (!result.IsSuccess)
            {
                await transaction.RollbackAsync(cancellation);
                return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
            }

            var newSaleOffer = new SaleOffer
            {
                RoomId = result.Value!.RoomId,
                ProposedByUserId = UserId,
                SellerUserId = productOwnerId,
                ItemId = itemId,
                QuantityRequested = proposedItem.SalePayload.QuantityRequested,
                PricePerUnit = proposedItem.SalePayload.ProposedPricePerunit,
                Status = SaleOfferStatus.Proposed,
                Version = 1,
                CreatedAt = DateTime.UtcNow
            };

            await _db.SaleOffers.AddAsync(newSaleOffer, cancellation);
            await _db.SaveChangesAsync(cancellation);

            var initialEvent = new SaleOfferEvent
            {
                SaleOfferId = newSaleOffer.Id,
                Version = 1,
                FromStatus = null,
                ToStatus = SaleOfferStatus.Proposed,
                PricePerUnit = newSaleOffer.PricePerUnit,
                QuantityRequested = newSaleOffer.QuantityRequested,
                ActorUserId = UserId,
                CreatedAt = newSaleOffer.CreatedAt
            };

            await _db.SaleOfferEvents.AddAsync(initialEvent, cancellation);
            await _db.SaveChangesAsync(cancellation);

            OfferPayload offerPayload = new OfferPayload(OfferTye.Sale, OfferStatus.Proposed, newSaleOffer.Id);
            SendMessage sendMessage = new SendMessage(
                _hasher.CreateHashids(result.Value!.RoomId, HashContext.Room),
                "Offer proposed message",
                null,
                MessageType.OfferProposed,
                offerPayload
            );

            UnifiedChat.MessageCommand messageCommand = new UnifiedChat.MessageCommand(UserId, sendMessage);
            var messageResult = await _mediator.Send(messageCommand, cancellation);
            if (!messageResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellation);
                return Result<MessageResponseDTO>.Failure(messageResult.Error!, messageResult.StatusCode);
            }

            await _db.OutboxEntries.AddAsync(
                new OutboxEntry
                {
                    EntityType = DTOs.Documentation.DocumentTarget.Product,
                    EntityId = itemId
                },
                cancellation
            );

            await _db.SaveChangesAsync(cancellation);
            await transaction.CommitAsync(cancellation);

            return Result<MessageResponseDTO>.Success(messageResult.Value!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error occurred while handling ProposedStrategyHandler for user {UserId}. Details: {@ProposedItem}", UserId, proposedItem);
            await transaction.RollbackAsync(cancellation);
            return Result<MessageResponseDTO>.Failure("An unexpected error occurred in our server.", StatusCodes.Status500InternalServerError);
        }
    }
}