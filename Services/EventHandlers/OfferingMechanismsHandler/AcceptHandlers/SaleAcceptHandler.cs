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

public class SaleAcceptOfferStrategy : IAcceptOfferStrategy
{
    public OfferTye Target => OfferTye.Sale;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    private readonly ILogger<SaleAcceptOfferStrategy> _logger;

    public SaleAcceptOfferStrategy(DbManager db, IHasher hasher, IMediator mediator, ILogger<SaleAcceptOfferStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<MessageResponseDTO>> AcceptStrategy(int UserId, AcceptItemDTO itemDTO, CancellationToken cancellationToken)
    {
        var decoded = _hasher.DecodeHashids(itemDTO.ParentOfferId, HashContext.SaleOffer);
        int offerId = decoded.Value;
        var offer = await _db.SaleOffers
            .Where(s => s.Id == offerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!offer!.TransitionTo(SaleOfferStatus.Accepted))
        {
            return Result<MessageResponseDTO>.Failure("Request is not allowed in current status of transaction.", StatusCodes.Status400BadRequest);
        }

        if (offer.ProposedByUserId == UserId)
        {
            return Result<MessageResponseDTO>.Failure("You cannot accept your own offer.", StatusCodes.Status400BadRequest);
        }

        if (offer.SellerUserId != UserId)
        {
            return Result<MessageResponseDTO>.Failure("You are not authorized to accept this offer.", StatusCodes.Status403Forbidden);
        }

        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var previousStatus = offer.Status;
            offer.Status = SaleOfferStatus.Accepted;
            offer.RespondedAt = DateTime.UtcNow;
            offer.Version += 1;

            var offerEvent = new SaleOfferEvent
            {
                SaleOfferId = offer.Id,
                Version = offer.Version,
                FromStatus = previousStatus,
                ToStatus = SaleOfferStatus.Accepted,
                PricePerUnit = offer.PricePerUnit,
                QuantityRequested = offer.QuantityRequested,
                ActorUserId = UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.SaleOfferEvents.AddAsync(offerEvent, cancellationToken);

            GetRoomDataCommand command = new GetRoomDataCommand(UserId, null, _hasher.CreateHashids(offer.RoomId, HashContext.Room));
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
            }

            OfferPayload offerPayload = new OfferPayload(OfferTye.Sale, OfferStatus.Accepted, offer.Id);
            SendMessage sendMessage = new SendMessage(
                _hasher.CreateHashids(result.Value!.RoomId, HashContext.Room),
                "Offer Accepted",
                null,
                MessageType.OfferAccepted,
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
            _logger.LogWarning(ex, "Concurrency conflict when accepting offer {OfferId} by user {UserId}.", offer.Id, UserId);
            return Result<MessageResponseDTO>.Failure("The offer was updated or responded to by another action. Please refresh.", StatusCodes.Status409Conflict);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(e, "An unexpected error occurred while handling SaleAcceptedHandler. UserId: {UserId}, ItemDetails: {@ItemDetails}", UserId, itemDTO);
            return Result<MessageResponseDTO>.Failure("An unexpected error occurred in our server.", StatusCodes.Status500InternalServerError);
        }
    }
}