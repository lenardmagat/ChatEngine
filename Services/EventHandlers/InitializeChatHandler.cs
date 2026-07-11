using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using System.Linq.Expressions;
using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.core;
namespace ChatSystem.Features.Chats;
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
        int? targetRoomId = !string.IsNullOrEmpty(command.ChatId) ? _hasher.DecodeHashids(command.ChatId, HashContext.Room).Value : null;
        int? targetReceiverID = !string.IsNullOrEmpty(command.RecieverId) ? _hasher.DecodeHashids(command.RecieverId, HashContext.User).Value : null;
        var query = _db.Chatrooms.AsNoTracking();
        if (targetRoomId.HasValue)
        {
            query = query.Where(r => r.Id == targetRoomId);
        }
        else
        {
            query = query
                .Where(c => 
                    c.Participants.Any(p => p.UserId == command.UserId) &&
                    c.Participants.Any(p => p.UserId == targetReceiverID)
                    );
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
            ).FirstOrDefaultAsync();
        if(ChatDataProjection is null)
        {
            if (targetRoomId.HasValue)
            {
                return Result<ChatData?>.Failure("Chat room session no longer exists.", StatusCodes.Status401Unauthorized);
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