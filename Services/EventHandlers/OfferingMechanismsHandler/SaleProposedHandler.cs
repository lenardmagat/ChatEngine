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
    List<SaleOfferStatus> NotAllowedStatus = new List<SaleOfferStatus> {SaleOfferStatus.Proposed, SaleOfferStatus.Countered, SaleOfferStatus.Accepted};
    public async Task<Result<MessageResponseDTO>> ProposedStrategy(int UserId, ProposedItemDTO proposedItem, CancellationToken cancellation)
    {
        if(proposedItem.SalePayload is null)
        {
            return Result<MessageResponseDTO>.Failure($"Invalid request: Missing data", StatusCodes.Status400BadRequest);
        }
        var Decoded = _hasher.DecodeOrFail(proposedItem.ItemId, HashContext.Product);
        int ItemId = Decoded.Value;
        var IsAlreadyHasOffer = await _db.SaleOffers
            .AsNoTracking()
            .Where(i => 
                i.ProposedByUserId == UserId && 
                i.RespondedAt == null &&
                i.ItemId == ItemId &&
                NotAllowedStatus.Contains(i.Status))
            .FirstOrDefaultAsync(cancellation);
        if(IsAlreadyHasOffer is not null){
            return Result<MessageResponseDTO>.Failure($"You already has ongoing transaction in this Item.", StatusCodes.Status400BadRequest);
        }
        var product = await _db.Products
            .Include(p => p.Owner)
            .AsNoTracking()
            .Where(p => p.Id == ItemId)
            .FirstOrDefaultAsync(cancellation);
        var productOwnerId = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == ItemId)
            .Select(p => p.Owner.UserId)
            .FirstOrDefaultAsync(cancellation);
        using var Transaction = await _db.Database.BeginTransactionAsync(cancellation);
        try{    
            int affectedRow = await _db.Products.Where(p => p.Id == ItemId && p.ProductAvailable >= proposedItem.SalePayload.QuantityRequested)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.ProductAvailable, p => p.ProductAvailable - proposedItem.SalePayload.QuantityRequested)
                    .SetProperty(p => p.ReservedProdcut, p => p.ReservedProdcut + proposedItem.SalePayload.QuantityRequested)
                    );
            if(affectedRow == 0)
            {
                return Result<MessageResponseDTO>.Failure("The item is does not have enough stock for request", StatusCodes.Status400BadRequest);
            }
            GetRoomDataCommand command = new GetRoomDataCommand(UserId, _hasher.CreateHashids(productOwnerId, HashContext.User), null);
            var result = await _mediator.Send(command, cancellation);
            if (!result.IsSuccess)
            {
                return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
            } 
            SaleOffer NewSaleOffer = new SaleOffer
            {
                RoomId = result.Value!.RoomId,
                ProposedByUserId = UserId,
                ItemId = ItemId,
                QuantityRequested = proposedItem.SalePayload.QuantityRequested,
                PricePerUnit = proposedItem.SalePayload.ProposedPricePerunit
            };
            await _db.SaleOffers.AddAsync(NewSaleOffer, cancellation);
            await _db.SaveChangesAsync(cancellation);
            OfferPayload offerPayload = new OfferPayload(OfferTye.Sale, OfferStatus.Proposed, NewSaleOffer.Id);
            SendMessage sendMessage = new SendMessage(_hasher.CreateHashids(result.Value!.RoomId, HashContext.Room), "Offer proposed message", null, MessageType.OfferProposed, offerPayload);
            UnifiedChat.MessageCommand messageCommand = new UnifiedChat.MessageCommand(UserId, sendMessage);
            var MessageResult = await _mediator.Send(messageCommand, cancellation);
            if (!MessageResult.IsSuccess)
            {
                return Result<MessageResponseDTO>.Failure(MessageResult.Error!, MessageResult.StatusCode);
            }
            await _db.OutboxEntries.AddAsync(
                new OutboxEntry
                {
                    EntityType = DTOs.Documentation.DocumentTarget.Product,
                    EntityId = ItemId
                }
            );
            await _db.SaveChangesAsync(cancellation);
            await Transaction.CommitAsync(cancellation);
            return  Result<MessageResponseDTO>.Success(MessageResult.Value!);
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"an unexpected error occured while handling ProposedStrategyHandler on user {UserId}. Details:{proposedItem}");
            await Transaction.RollbackAsync(cancellation);
            return  Result<MessageResponseDTO>.Failure("An unexpected error occured in our server.", StatusCodes.Status500InternalServerError);
        }
    }

}