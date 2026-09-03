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
    public SaleAcceptOfferStrategy(DbManager db , IHasher hasher, IMediator mediator, ILogger<SaleAcceptOfferStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
    public async Task<Result<MessageResponseDTO>> AcceptStrategy(int UserId, AcceptItemDTO itemDTO, CancellationToken cancellationToken)
    {
        var ParentOfferId = _hasher.DecodeHashids(itemDTO.ParentOfferId, HashContext.SaleOffer).Value;
        var ParentOffer = await _db.SaleOffers.AsNoTracking().Where(s => s.Id == ParentOfferId).FirstOrDefaultAsync(cancellationToken);
        if (!ParentOffer!.TransitionTo(Models.SaleOfferStatus.Accepted))
        {
            return Result<MessageResponseDTO>.Failure("Request is not allowed in current status of transaction.", StatusCodes.Status400BadRequest);
        }
        if(ParentOffer.ProposedByUserId == UserId)
        {
             return Result<MessageResponseDTO>.Failure("You cannot accept your own offer.", StatusCodes.Status400BadRequest);
        }
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            SaleOffer newOffer = new SaleOffer
            {
                RoomId = ParentOffer.RoomId,
                ProposedByUserId = ParentOffer.ProposedByUserId,
                ParentId = ParentOffer.Id,
                ItemId = ParentOffer.ItemId,
                QuantityRequested = ParentOffer.QuantityRequested,
                PricePerUnit = ParentOffer.PricePerUnit,
                Status = SaleOfferStatus.Accepted
            };
            await _db.SaleOffers.AddAsync(newOffer, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _db.SaleOffers
                .Where(p => p.Id == ParentOffer.Id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.RespondedAt, DateTime.UtcNow)
                    .SetProperty(p => p.Status, SaleOfferStatus.Accepted),
                    cancellationToken
                );
            GetRoomDataCommand command = new GetRoomDataCommand(UserId, null,  _hasher.CreateHashids(ParentOffer.RoomId, HashContext.Room));
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
            }
            OfferPayload offerPayload = new OfferPayload( OfferTye.Sale, OfferStatus.Accepted, newOffer.Id);
            SendMessage sendMessage = new SendMessage(_hasher.CreateHashids(result.Value!.RoomId, HashContext.Room), "Offer Accepted", null, MessageType.OfferAccepted, offerPayload);
            UnifiedChat.MessageCommand messageCommand = new UnifiedChat.MessageCommand(UserId, sendMessage);
            var MessageResult = await _mediator.Send(messageCommand, cancellationToken);
            if (!MessageResult.IsSuccess)
            {
                return Result<MessageResponseDTO>.Failure(MessageResult.Error!, MessageResult.StatusCode);
            }
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return MessageResult;
            
        }catch(Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(e, "An unexpected error occurred while handling SaleAcceptedHandler. UserId: {UserId}, ItemDetails: {@ItemDetails}", UserId, itemDTO);
            return Result<MessageResponseDTO>.Failure("An unexpected error occurred in our server.", StatusCodes.Status500InternalServerError);
        }
    }   
}