using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Chats;
using MediatR;

namespace ChatSystem.EventHandler.Chats;
public class SendMessageTextStrategy : IMessageStrategy
{
    public MessageType Target => MessageType.Text;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    private readonly ILogger<SendMessageTextStrategy> _logger;
    public SendMessageTextStrategy(IMediator mediator, DbManager db, IHasher hasher, ILogger<SendMessageTextStrategy> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
    public async Task<Result<MessageResponseDTO>> MessageHandler(int UserId, SendMessage request, CancellationToken cancellation)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(cancellation);
        GetRoomDataCommand command = new GetRoomDataCommand(UserId, request.RecieverId, request.RoomId);
            var RoomDataResult = await _mediator.Send(command, cancellation);
            if (!RoomDataResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellation);
                return Result<MessageResponseDTO>.Failure(RoomDataResult.Error!, RoomDataResult.StatusCode);
            }
        try
        {
            
            
            var RoomData = RoomDataResult.Value;
            var newMessage = new ChatMessage
            {
                RoomId = RoomData!.RoomId,
                SenderId = UserId,
                MessageText = request.Message,
                TimeStamp = DateTime.UtcNow,
                Type = MessageType.Text
            };
            await _db.Messages.AddAsync(newMessage, cancellation);
            await _db.SaveChangesAsync(cancellation);
            await transaction.CommitAsync(cancellation);
            return Result<MessageResponseDTO>.Success(new MessageResponseDTO
                    (
                    _hasher.CreateHashids(RoomData!.RoomId, HashContext.Room),
                    _hasher.CreateHashids(RoomData!.ReceiverId, HashContext.User),
                    new MessageData
                        (
                        _hasher.CreateHashids(newMessage.Id, HashContext.Message),
                        newMessage.MessageText,
                        newMessage.TimeStamp,
                        RoomData.ReceiverUsername,
                        _hasher.CreateHashids(newMessage.SenderId, HashContext.User),
                        null,
                        newMessage.Type,
                        null           
                    )
                )
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellation);
            _logger.LogError(ex, "Error occurred while sending text message.");
            return Result<MessageResponseDTO>.Failure("An error occurred while sending the message.", 500);
        }
    }
}