using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using System.Linq.Expressions;
using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.core;
namespace ChatSystem.EventHandler.Chats;
public static class ChatProjections
{
    public static Expression<Func<ChatMessage, MessageSummaryDto>> ToSummary()
    {
        return m => new MessageSummaryDto
        {
            ChatId = m.Id,
            SenderName = m.Sender.Username,
            SenderId = m.SenderId,
            ChatMessage = m.MessageText,
            TimeStampt = m.TimeStamp
        };
    }
}
public class InitializeChatCommandHandler : IRequestHandler<InitializeChatCommand, Result<ChatData?>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public InitializeChatCommandHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<ChatData?>> Handle(InitializeChatCommand command, CancellationToken cancellation)
    {
        Result<int>? decodedRoomId = !string.IsNullOrEmpty(command.ChatId) 
            ? _hasher.DecodeOrFail(command.ChatId, HashContext.Room) 
            : null;
        if (decodedRoomId is not null && !decodedRoomId.IsSuccess)
        {
            return Result<ChatData?>.Failure(decodedRoomId.Error!, decodedRoomId.StatusCode);
        }

        Result<int>? decodedReceiverId = !string.IsNullOrEmpty(command.RecieverId) 
            ? _hasher.DecodeOrFail(command.RecieverId, HashContext.User) 
            : null;
        if (decodedReceiverId is not null && !decodedReceiverId.IsSuccess)
        {
            return Result<ChatData?>.Failure(decodedReceiverId.Error!, decodedReceiverId.StatusCode);
        }

        int? targetRoomId = decodedRoomId?.Value;
        int? targetReceiverID = decodedReceiverId?.Value;

        var query = _db.Chatrooms.AsNoTracking();
        if (targetRoomId.HasValue)
        {
            query = query.Where(r => r.Id == targetRoomId.Value && r.Participants.Any(p => p.UserId == command.UserId));
        }
        else if (targetReceiverID.HasValue)
        {
            query = query
                .Where(c => 
                    c.Participants.Any(p => p.UserId == command.UserId) &&
                    c.Participants.Any(p => p.UserId == targetReceiverID.Value)
                    );
        }
        else
        {
            return Result<ChatData?>.Failure("Either ChatId or RecieverId must be specified.", StatusCodes.Status400BadRequest);
        }

        var ChatDataProjection = await query
            .Select(r => new
            {
                RoomId = r.Id,
                ReceiverId = r.Participants
                    .Where(p => p.UserId != command.UserId)
                    .Select(u => u.UserId)
                    .FirstOrDefault(),
                LastMessageTimeStampt = r.Messages
                    .Max(m => m.TimeStamp),
                RecentMessages = r.Messages
                    .OrderByDescending(m => m.Id)
                    .Take(10)
                    .AsQueryable()
                    .Select(ChatProjections.ToSummary())
                    .ToList()
            }
            ).FirstOrDefaultAsync(cancellation);
        if(ChatDataProjection is null)
        {
            if (targetRoomId.HasValue)
            {
                bool roomExists = await _db.Chatrooms.AnyAsync(r => r.Id == targetRoomId.Value, cancellation);
                if (roomExists)
                {
                    return Result<ChatData?>.Failure("You do not have permission to access this chat room.", StatusCodes.Status403Forbidden);
                }
                return Result<ChatData?>.Failure("Chat room session no longer exists.", StatusCodes.Status404NotFound);
            }
            return Result<ChatData?>.Success(new ChatData(true, null ,null ,null ,null));
        }
        List<MessageData> messageDatas = ChatDataProjection
            .RecentMessages
            .Select(m => new MessageData(
                _hasher.CreateHashids(m.ChatId, HashContext.Message),
                m.ChatMessage,
                m.TimeStampt,
                m.SenderName,
                _hasher.CreateHashids(m.SenderId, HashContext.User)
                )
            ).ToList();
        ChatData data = new ChatData(
            false,
            _hasher.CreateHashids(ChatDataProjection.RoomId, HashContext.Room),
            ChatDataProjection.LastMessageTimeStampt,
            _hasher.CreateHashids(ChatDataProjection.ReceiverId, HashContext.User),
            messageDatas
        );
        return Result<ChatData?>.Success(data);
    }
}