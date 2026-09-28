using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Chats;
public class SendMessageProposedStrategy : IMessageStrategy
{
    public MessageType Target => MessageType.OfferProposed;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    ILogger<SendMessageProposedStrategy> _logger;
    public SendMessageProposedStrategy(IMediator mediator, DbManager db, IHasher hasher, ILogger<SendMessageProposedStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
     public async Task<Result<MessageResponseDTO>> MessageHandler(int UserId, SendMessage request, CancellationToken cancellation)
    {
        if(request.OfferPayload!.Tye == OfferTye.Sale)
        {
            try
            {
                GetRoomDataCommand command = new GetRoomDataCommand(UserId, request.RecieverId, request.RoomId);
                var RoomDataResult = await _mediator.Send(command, cancellation);
                if (!RoomDataResult.IsSuccess)
                {
                    return Result<MessageResponseDTO>.Failure(RoomDataResult.Error!, RoomDataResult.StatusCode);
                }
                var roomData = RoomDataResult.Value;
                ChatMessage Newmessage = new ChatMessage
                {
                    RoomId = _hasher.DecodeHashids(request.RoomId!, HashContext.Room).Value,
                    SenderId = UserId,
                    MessageText = "Sale offer proposed",
                    SaleOfferId = request.OfferPayload.offerId,
                    SaleOfferEventId = request.OfferPayload.SaleOfferEventId
                };
                await _db.Messages.AddAsync(Newmessage, cancellation);
                await _db.SaveChangesAsync(cancellation);

                var saleOfferEvent = await _db.SaleOfferEvents
                    .AsNoTracking()
                    .Where(e => e.Id == request.OfferPayload.SaleOfferEventId)
                    .Select(e => new
                    {
                        OfferId = e.SaleOfferId,
                        ItemId = e.SaleOffer.ItemId,
                        ItemName = e.SaleOffer.ItemDetails.ProductName,
                        e.QuantityRequested,
                        e.PricePerUnit,
                        Status = e.ToStatus,
                        ProposedByUsername = e.Actor.Username,
                        e.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellation);

                MessageResponseDTO messageResponse = new MessageResponseDTO(
                    _hasher.CreateHashids(Newmessage.RoomId, HashContext.Room),
                    _hasher.CreateHashids(roomData!.ReceiverId, HashContext.User),
                    new MessageData(
                        _hasher.CreateHashids(Newmessage.Id, HashContext.Message),
                        Newmessage.MessageText,
                        Newmessage.TimeStamp,
                        roomData.ReceiverUsername, // Sender username — already fetched via GetRoomDataCommand
                        _hasher.CreateHashids(Newmessage.SenderId, HashContext.User),
                        new SaleOfferResponseDTO(
                            _hasher.CreateHashids(saleOfferEvent!.OfferId, HashContext.SaleOffer),
                            _hasher.CreateHashids(saleOfferEvent.ItemId, HashContext.Product),
                            saleOfferEvent.ItemName,
                            saleOfferEvent.QuantityRequested,
                            saleOfferEvent.PricePerUnit,
                            saleOfferEvent.PricePerUnit * saleOfferEvent.QuantityRequested,
                            saleOfferEvent.Status.ToString(),
                            saleOfferEvent.ProposedByUsername,
                            saleOfferEvent.CreatedAt
                        ),
                        MessageType.OfferProposed,
                        OfferTye.Sale
                    )
                );
                return Result<MessageResponseDTO>.Success(messageResponse);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Unexpected error occurred while handling SendMessageProposedStrategy. Details: {@Request}", request);
                return Result<MessageResponseDTO>.Failure("An unexpected error occurred in our server.", StatusCodes.Status500InternalServerError);
            }
        }
        return Result<MessageResponseDTO>.Failure("Unsupported offer type.", StatusCodes.Status400BadRequest);
    }
}