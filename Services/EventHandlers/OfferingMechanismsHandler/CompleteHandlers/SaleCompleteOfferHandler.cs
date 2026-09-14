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

namespace ChatSystem.EventHandler.OfferingMechanisms;
public class SaleCompleteOfferStrategy : ICompleteStrategy
{
    public OfferTye Target => OfferTye.Sale;
    private readonly IMediator _mediator;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly ILogger<SaleCompleteOfferStrategy> _logger;
    public SaleCompleteOfferStrategy(DbManager db, IHasher hasher, IMediator mediator, ILogger<SaleCompleteOfferStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
    public async Task<Result<MessageResponseDTO>> CompleteStrategy(int UserId, CompleteOfferDTO completeOfferDTO, CancellationToken cancellationToken)
    {
        var offerId = _hasher.DecodeHashids(completeOfferDTO.ParentOfferId, HashContext.SaleOffer).Value;
        var offer = await _db.SaleOffers
            .Include(o => o.Room)
                .ThenInclude(r => r.Participants)
            .Where(s => s.Id == offerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!offer!.TransitionTo(SaleOfferStatus.Completed))
        {
            return Result<MessageResponseDTO>.Failure("The offer cannot be complete, due to its current state.", StatusCodes.Status400BadRequest);
        }
        Roles UserRole = await _db.Users
            .Where(u => u.UserId == UserId)
            .Select(d => d.Role)
            .FirstOrDefaultAsync(cancellationToken);
        if(offer.Room.Participants.Any(p => p.UserId == UserId) == false || UserRole == Roles.System)
        {
            return Result<MessageResponseDTO>.Failure("You are not authorized to complete this offer.", StatusCodes.Status403Forbidden);
        }
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var previousStatus = offer.Status;
            offer.Status = SaleOfferStatus.Completed;
            offer.RespondedAt = DateTime.UtcNow;
            offer.Version += 1;
            var offerEvent = new SaleOfferEvent
            {
                SaleOfferId = offer.Id,
                Version = offer.Version,
                FromStatus = previousStatus,
                ToStatus = SaleOfferStatus.Completed,
                PricePerUnit = offer.PricePerUnit,
                QuantityRequested = offer.QuantityRequested,
                ActorUserId = UserId,
                CreatedAt = DateTime.UtcNow
            };
            await _db.SaleOfferEvents.AddAsync(offerEvent, cancellationToken);

            GetRoomDataCommand roomDataCommand = new GetRoomDataCommand(UserId, null, _hasher.CreateHashids(offer.RoomId, HashContext.Room));
            var RoomDataResult = await _mediator.Send(roomDataCommand, cancellationToken);
            if (!RoomDataResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MessageResponseDTO>.Failure(RoomDataResult.Error!, RoomDataResult.StatusCode);
            }

            OfferPayload offerPayload = new OfferPayload(OfferTye.Sale, OfferStatus.Completed, offer.Id);
            SendMessage sendMessage = new SendMessage(
                _hasher.CreateHashids(RoomDataResult.Value!.RoomId, HashContext.Room),
                "Transaction Complete!",
                null,
                MessageType.OfferCompleted,
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
        catch(Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "An error occurred while completing the sale offer with ID {OfferId}.", offerId);
            return Result<MessageResponseDTO>.Failure("An unexpected error occurred while processing your request.", StatusCodes.Status500InternalServerError);
        }
    }
}