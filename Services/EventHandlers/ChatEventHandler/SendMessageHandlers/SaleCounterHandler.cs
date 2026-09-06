using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.chat;
public class SendMessageCounterStrategy : IMessageStrategy
{
    public MessageType Target => MessageType.OfferCountered;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    ILogger<SendMessageCounterStrategy> _logger;
    public SendMessageCounterStrategy(IMediator mediator, DbManager db, IHasher hasher, ILogger<SendMessageCounterStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
    public async Task<Result<MessageResponseDTO>> MessageHandler(int UserId, SendMessage request, CancellationToken cancellation)
    {
        try
        {
            GetRoomDataCommand command = new GetRoomDataCommand(UserId, request.RecieverId, request.RoomId);
            var RoomDataResult = await _mediator.Send(command, cancellation);
            if (!RoomDataResult.IsSuccess)
            {
                return Result<MessageResponseDTO>.Failure(RoomDataResult.Error!, RoomDataResult.StatusCode);
            }
            var RoomData = RoomDataResult.Value;
            var newMessage = new ChatMessage
            {
                RoomId = RoomData!.RoomId,
                SenderId = UserId,
                MessageText = "Sale offer countered",
                TimeStamp = DateTime.UtcNow,
                Type = MessageType.OfferCountered,
                SaleOfferId = request.OfferPayload!.offerId
            };
            await _db.Messages.AddAsync(newMessage, cancellation);
            await _db.SaveChangesAsync(cancellation);
            var saleOffer = await _db.SaleOffers
                    .AsNoTracking()
                    .Where(s => s.Id == request.OfferPayload.offerId)
                    .Select(s => new
                    {
                        s.Id,
                        s.ItemId,
                        ItemName = s.ItemDetails.ProductName,
                        s.QuantityRequested,
                        s.PricePerUnit,
                        s.Status
                    })
                    .FirstOrDefaultAsync(cancellation);
            MessageResponseDTO messageResponse = new MessageResponseDTO(
                _hasher.CreateHashids(RoomData.RoomId, HashContext.Room),
                _hasher.CreateHashids(RoomData.ReceiverId, HashContext.User),
                new MessageData(
                    _hasher.CreateHashids(newMessage.Id, HashContext.Message),
                    newMessage.MessageText,
                    newMessage.TimeStamp,
                    RoomData.ReceiverUsername,
                    _hasher.CreateHashids(newMessage.SenderId, HashContext.User),
                    new SaleOfferResponseDTO(
                        _hasher.CreateHashids(saleOffer!.Id, HashContext.SaleOffer),
                        _hasher.CreateHashids(saleOffer.ItemId, HashContext.Product),
                        saleOffer.ItemName,
                        saleOffer.QuantityRequested,
                        saleOffer.PricePerUnit,
                        saleOffer.PricePerUnit * saleOffer.QuantityRequested,
                        saleOffer.Status.ToString(),
                        RoomData.ReceiverUsername,
                        DateTime.UtcNow
                        ),
                    newMessage.Type,
                    OfferTye.Sale
                    )
                );
            return Result<MessageResponseDTO>.Success(messageResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while handling message of type {MessageType}", Target);
            return Result<MessageResponseDTO>.Failure("An error occurred while processing the message.", 500);
        }
    }
}