using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChatSystem.EventHandler.Chats;
public class SendMessageOfferAcceptedStrategy : IMessageStrategy
{
    public MessageType Target => MessageType.OfferAccepted;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    private readonly ILogger<SendMessageOfferAcceptedStrategy> _logger;
    public SendMessageOfferAcceptedStrategy(IMediator mediator, DbManager db, IHasher hasher, ILogger<SendMessageOfferAcceptedStrategy> logger)
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
                MessageText = request.Message,
                TimeStamp = DateTime.UtcNow,
                Type = MessageType.OfferAccepted,
                SaleOfferId = request.OfferPayload!.offerId,
                SaleOfferEventId = request.OfferPayload.SaleOfferEventId
            };
            await _db.Messages.AddAsync(newMessage, cancellation);
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
            MessageResponseDTO  messageResponseDTO = new MessageResponseDTO(
                _hasher.CreateHashids(newMessage.RoomId, HashContext.Room),
                _hasher.CreateHashids(RoomData.ReceiverId, HashContext.User),
                new MessageData(
                    _hasher.CreateHashids(newMessage.Id, HashContext.Message),
                    newMessage.MessageText,
                    newMessage.TimeStamp,
                    RoomData.ReceiverUsername, // Sender username — already fetched via GetRoomDataCommand
                        _hasher.CreateHashids(newMessage.SenderId, HashContext.User),
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
                    MessageType.OfferAccepted,
                    OfferTye.Sale
                )
            );
            return Result<MessageResponseDTO>.Success(messageResponseDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message.");
            return Result<MessageResponseDTO>.Failure("An error occurred while sending the message.", StatusCodes.Status500InternalServerError);
        }
    }
}