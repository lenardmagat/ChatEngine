using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using Microsoft.EntityFrameworkCore;
using MediatR;
using ChatSystem.Models;
using System.Data;
using ChatSystem.SystemEvents.Chats;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ChatSystem.EventHandler.OfferingMechanisms;
public class SaleCounterOfferHandler : ICounterOfferStrategy
{
    public OfferTye Target => OfferTye.Sale;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public SaleCounterOfferHandler(DbManager db, IHasher hasher, IMediator mediator, ILogger logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
    public async Task<Result<MessageResponseDTO>> CounterOfferStrategy(int UserId, CounterOfferDTO data, CancellationToken cancellationToken)
    {
        if(data.SalePayload is null)
        {
            return Result<MessageResponseDTO>.Failure("Invalid request: Missing data", StatusCodes.Status400BadRequest);
        }

        if(data.SalePayload.QuantityRequested <= 0)
        {
            return Result<MessageResponseDTO>.Failure("Quantity requested must be greater than zero.", StatusCodes.Status400BadRequest);
        }

        if(data.SalePayload.ProposedPricePerunit < 0)
        {
            return Result<MessageResponseDTO>.Failure("Proposed price per unit cannot be negative.", StatusCodes.Status400BadRequest);
        }
        var ItemId = _hasher.DecodeOrFail(data.ItemId, HashContext.Product).Value;
        var ParentOfferId = _hasher.DecodeOrFail(data.ParentOfferId, HashContext.SaleOffer).Value;
        var existingOffer =  await _db.SaleOffers.
            Include(o => o.Room)
                .ThenInclude(r => r.Participants)
            .Where(o => o.Id == ParentOfferId && o.ItemId == ItemId)
            .FirstOrDefaultAsync(cancellationToken);
        if(existingOffer!.Room.Participants.Any(p => p.UserId == UserId) == false)
        {
            return Result<MessageResponseDTO>.Failure("You are not authorized to counter this offer.", StatusCodes.Status403Forbidden);
        }   
        if(!new List<SaleOfferStatus>{SaleOfferStatus.Proposed, SaleOfferStatus.Countered}.Contains(existingOffer!.Status))
        {
            return Result<MessageResponseDTO>.Failure("Request is not allowed in current status of transaction.", StatusCodes.Status400BadRequest);
        }
        if(existingOffer.ProposedByUserId == UserId && existingOffer.Status == SaleOfferStatus.Countered)
        {
            return Result<MessageResponseDTO>.Failure("You cannot counter your own counter offer.", StatusCodes.Status400BadRequest);
        }
        
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            int quantityDelta = data.SalePayload.QuantityRequested >= existingOffer.QuantityRequested 
                ? -(data.SalePayload.QuantityRequested - existingOffer.QuantityRequested)
                : (existingOffer.QuantityRequested - data.SalePayload.QuantityRequested);
            int rowsAffected = await _db.Products.ExecuteUpdateAsync(setter => setter
                .SetProperty(p => p.ProductAvailable, p => p.ProductAvailable + quantityDelta)
                .SetProperty(p => p.ReservedProdcut, p => p.ReservedProdcut + quantityDelta)
                .SetProperty(p => p.UpdatedA, p => DateTime.UtcNow)
            , cancellationToken);
            if(rowsAffected == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure("The item does not have enough stock for this request.", StatusCodes.Status400BadRequest);
            }
            var prevStatus = existingOffer.Status;
            existingOffer.Status = SaleOfferStatus.Countered;
            existingOffer.RespondedAt = DateTime.UtcNow;
            existingOffer.Version += 1;
            existingOffer.PricePerUnit = data.SalePayload.ProposedPricePerunit;
            existingOffer.QuantityRequested = data.SalePayload.QuantityRequested;
            SaleOfferEvent offerEvent = new SaleOfferEvent
            {
                SaleOfferId = existingOffer.Id,
                Version = existingOffer.Version,
                FromStatus = prevStatus,
                ToStatus = SaleOfferStatus.Countered,
                PricePerUnit = existingOffer.PricePerUnit,
                QuantityRequested = existingOffer.QuantityRequested,
                ActorUserId = UserId
            };
            await _db.SaleOfferEvents.AddAsync(offerEvent, cancellationToken);
            GetRoomDataCommand command = new GetRoomDataCommand(UserId, null, _hasher.CreateHashids(existingOffer.RoomId, HashContext.Room));
            var result = await _mediator.Send(command, cancellationToken);
            if(!result.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
            }
            SendMessage message = new SendMessage(
                _hasher.CreateHashids(result.Value!.RoomId, HashContext.Room),
                "Sale offer countered",
                null,
                MessageType.OfferCountered,
                new OfferPayload(
                    OfferTye.Sale,
                    OfferStatus.Countered,
                    existingOffer.Id
                )
            );
            SendMessageCommand messageCommand = new SendMessageCommand(UserId, message);
            var messageResult = await _mediator.Send(messageCommand, cancellationToken);
            if(!messageResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure(messageResult.Error!, messageResult.StatusCode);
            }
            _db.OutboxEntries.Add(new OutboxEntry
            {
                EntityId = ItemId,
                EntityType = DTOs.Documentation.DocumentTarget.Product,
            });
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<MessageResponseDTO>.Success(messageResult.Value!);
        }
        catch(DBConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency error occurred while handling SaleCounterOfferHandler. Details: {@Data}", data);
            return Result<MessageResponseDTO>.Failure("The offer has been modified by another user. Please refresh and try again.", StatusCodes.Status409Conflict);
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Unexpected error occurred while handling SaleCounterOfferHandler. Details: {@Data}", data);
            return Result<MessageResponseDTO>.Failure("An error occurred while processing the request.", StatusCodes.Status500InternalServerError);
        }
        
        
    }
}