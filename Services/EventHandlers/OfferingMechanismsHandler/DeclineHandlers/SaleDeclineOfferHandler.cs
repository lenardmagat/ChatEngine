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

public class SaleDeclineOfferStrategy : IDeclineOfferStrategy
{
    public OfferTye Target => OfferTye.Sale;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    private readonly ILogger<SaleDeclineOfferStrategy> _logger;

    public SaleDeclineOfferStrategy(DbManager db, IHasher hasher, IMediator mediator, ILogger<SaleDeclineOfferStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<MessageResponseDTO>> DeclineStrategy(int UserId, DeclineItemDTO itemDTO, CancellationToken cancellationToken)
    {
        var decoded = _hasher.DecodeOrFail(itemDTO.ParentOfferId, HashContext.SaleOffer);
        if (!decoded.IsSuccess)
        {
            return Result<MessageResponseDTO>.Failure(decoded.Error!, decoded.StatusCode);
        }

        int offerId = decoded.Value;
        var offer = await _db.SaleOffers
            .Include(o => o.Room)
                .ThenInclude(r => r.Participants)
            .Where(s => s.Id == offerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (offer is null)
        {
            return Result<MessageResponseDTO>.Failure("The offer does not exist.", StatusCodes.Status404NotFound);
        }

        if (!offer.TransitionTo(SaleOfferStatus.Declined))
        {
            return Result<MessageResponseDTO>.Failure("Request is not allowed in current status of transaction.", StatusCodes.Status400BadRequest);
        }

        var lastActorId = await _db.SaleOfferEvents
            .Where(e => e.SaleOfferId == offer.Id)
            .OrderByDescending(e => e.Version)
            .Select(e => e.ActorUserId)
            .FirstOrDefaultAsync(cancellationToken);

        int currentProposerId = lastActorId != 0 ? lastActorId : offer.ProposedByUserId;
        if (currentProposerId == UserId)
        {
            return Result<MessageResponseDTO>.Failure("You cannot decline your own offer.", StatusCodes.Status400BadRequest);
        }

        if (offer.Room.Participants.Any(p => p.UserId == UserId) == false)
        {
            return Result<MessageResponseDTO>.Failure("You are not authorized to decline this offer.", StatusCodes.Status403Forbidden);
        }

        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            int affectedProduct = await _db.Products
                .Where(p => p.Id == offer.ItemId)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.ProductAvailable, p => p.ProductAvailable + offer.QuantityRequested)
                    .SetProperty(p => p.ReservedProdcut, p => p.ReservedProdcut - offer.QuantityRequested)
                    .SetProperty(p => p.UpdatedA, DateTime.UtcNow),
                    cancellationToken
                );

            if (affectedProduct == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure("Failed to update product stock for declined offer.", StatusCodes.Status400BadRequest);
            }

            var previousStatus = offer.Status;
            offer.Status = SaleOfferStatus.Declined;
            offer.RespondedAt = DateTime.UtcNow;
            offer.Version += 1;

            var offerEvent = new SaleOfferEvent
            {
                SaleOfferId = offer.Id,
                Version = offer.Version,
                FromStatus = previousStatus,
                ToStatus = SaleOfferStatus.Declined,
                PricePerUnit = offer.PricePerUnit,
                QuantityRequested = offer.QuantityRequested,
                ActorUserId = UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.SaleOfferEvents.AddAsync(offerEvent, cancellationToken);

            await _db.OutboxEntries.AddAsync(
                new OutboxEntry
                {
                    EntityType = DTOs.Documentation.DocumentTarget.Product,
                    EntityId = offer.ItemId
                },
                cancellationToken
            );

            GetRoomDataCommand command = new GetRoomDataCommand(UserId, null, _hasher.CreateHashids(offer.RoomId, HashContext.Room));
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
            }

            OfferPayload offerPayload = new OfferPayload(OfferTye.Sale, OfferStatus.Declined, offer.Id);
            SendMessage sendMessage = new SendMessage(
                _hasher.CreateHashids(result.Value!.RoomId, HashContext.Room),
                "Offer Declined",
                null,
                MessageType.OfferDeclined,
                offerPayload
            );

            UnifiedChat.MessageCommand messageCommand = new UnifiedChat.MessageCommand(UserId, sendMessage);
            var messageResult = await _mediator.Send(messageCommand, cancellationToken);
            if (!messageResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure(messageResult.Error!, messageResult.StatusCode);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return messageResult;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Concurrency conflict when declining offer {OfferId} by user {UserId}.", offer.Id, UserId);
            return Result<MessageResponseDTO>.Failure("The offer was updated or responded to by another action. Please refresh.", StatusCodes.Status409Conflict);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(e, "An unexpected error occurred while handling SaleDeclineOfferStrategy. UserId: {UserId}, ItemDetails: {@ItemDetails}", UserId, itemDTO);
            return Result<MessageResponseDTO>.Failure("An unexpected error occurred in our server.", StatusCodes.Status500InternalServerError);
        }
    }
}
