using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Chats;
using MediatR;

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
                    SaleOfferId = request.OfferPayload.offerId
                };
                await _db.Messages.AddAsync(Newmessage, cancellation);
                await _db.SaveChangesAsync(cancellation);

                MessageResponseDTO messageResponse = new MessageResponseDTO(
                    _hasher.CreateHashids(Newmessage.RoomId, HashContext.Room),
                    _hasher.CreateHashids(roomData!.ReceiverId, HashContext.User),
                    new MessageData(
                        _hasher.CreateHashids(Newmessage.Id, HashContext.Message),
                        Newmessage.MessageText,
                        Newmessage.TimeStamp,
                        Newmessage.Sender.Username,
                        _hasher.CreateHashids(Newmessage.SenderId, HashContext.User),
                        new SaleOfferResponseDTO(
                            _hasher.CreateHashids(Newmessage.SaleOffer!.Id, HashContext.SaleOffer),
                            _hasher.CreateHashids(Newmessage.SaleOffer.ItemId, HashContext.Product),
                            Newmessage.SaleOffer.ItemDetails.ProductName,
                            Newmessage.SaleOffer.QuantityRequested,
                            Newmessage.SaleOffer.PricePerUnit,
                            Newmessage.SaleOffer.PricePerUnit * Newmessage.SaleOffer.QuantityRequested,
                            Newmessage.SaleOffer.Status.ToString(),
                            Newmessage.SaleOffer.UserProposed.Username,
                            Newmessage.SaleOffer.CreatedAt
                        ),
                        MessageType.OfferProposed
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