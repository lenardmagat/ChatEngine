using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.DataBase;
using ChatSystem.core;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;
using ChatSystem.Services.Interfaces;
namespace ChatSystem.EventHandler.Chats;
public class SendMessageCommandHandler : IMessageStrategy
{
    public MessageType Target => MessageType.Text;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly IMediator _mediator;
    ILogger<SendMessageCommandHandler> _logger;
    public SendMessageCommandHandler(IMediator mediator, DbManager db, IHasher hasher, ILogger<SendMessageCommandHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _mediator = mediator;
        _logger = logger;
    }
    public async Task<Result<MessageResponseDTO>> MessageHandler(int UserId, SendMessage request, CancellationToken cancellation)
    {
        
        bool isAlreadyInTransaction = _db.Database.CurrentTransaction != null!;
        IDbContextTransaction? localTransaction = null!;
        if (!isAlreadyInTransaction)
        {
            localTransaction = await _db.Database.BeginTransactionAsync(cancellation);
        }
        try
        {
            GetRoomDataCommand command = new GetRoomDataCommand(UserId, request.RecieverId, request.RoomId);
            var RoomDataResult = await _mediator.Send(command, cancellation);
            if (!RoomDataResult.IsSuccess)
            {
                await localTransaction.RollbackAsync(cancellation);
                return Result<MessageResponseDTO>.Failure(RoomDataResult.Error!, RoomDataResult.StatusCode);
            }
            var RoomData = RoomDataResult.Value;
            var newMessage = new ChatMessage
            {
                RoomId = RoomData!.RoomId,
                SenderId = UserId,
                MessageText = request.Message,
                TimeStamp = DateTime.UtcNow
            };
            await _db.Messages.AddAsync(newMessage);
            await _db.SaveChangesAsync(cancellation);
            if(localTransaction != null)
            {
                await localTransaction.CommitAsync(cancellation);
            }
            string newMessageHashedId = _hasher.CreateHashids(newMessage.Id, HashContext.Message);
            string hashedRoomId = _hasher.CreateHashids(RoomData.RoomId, HashContext.Room);
            string hashedRecipientId = _hasher.CreateHashids(RoomData.ReceiverId, HashContext.User);
            return Result<MessageResponseDTO>.Success( new MessageResponseDTO(
                newMessageHashedId, 
                hashedRoomId, 
                RoomData.ReceiverUsername, 
                request.Message, 
                newMessage.TimeStamp.ToString(), 
                hashedRecipientId
            )
        );
    }
    catch(Exception e)
    {
        _logger.LogError(e, $"Un handled error occured while handling SendMessageHandler. Details{request}");
        return Result<MessageResponseDTO>.Failure("An unxexpected occured in our server.", StatusCodes.Status500InternalServerError);
    }
}
        
        

        

    
}