using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Features.Chats;
public class LoadConversationHandler : IRequestHandler<LoadConversationCommand, Result<List<LoadConversationResponse>>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public LoadConversationHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<List<LoadConversationResponse>>> Handle(LoadConversationCommand request, CancellationToken cancellation)
    {
        var result = await _db.Chatrooms
                        .AsNoTracking()
                        .Where(c => c.Participants
                        .Any(p => p.UserId == request.UserId))
                        .OrderByDescending(r => r.Messages
                            .Max(m => m.TimeStamp)
                            )
                        .Take(15)
                        .Select(r => new
                        {
                            RoomId = r.Id,
                            recieverId = r.Participants
                                .Where(p => p.UserId != request.UserId)
                                .Select(u => u.UserId)
                                .First(),
                            RecentMessages = r.Messages
                                .OrderByDescending(m => m.Id)
                                .Take(15)
                                .Select(m => new
                                {
                                    chatId = m.Id,
                                    senderName = m.Sender.Username,
                                    senderId = m.SenderId,
                                    chatMessage = m.MessageText,
                                    timestampt = m.TimeStamp,
                                }
                                ).ToList()
                        }
                        ).ToListAsync();
        if(result is null)
        {
            return Result<List<LoadConversationResponse>>.Failure("Invalid Credentials", StatusCodes.Status401Unauthorized);
        }
        List<LoadConversationResponse> response = new List<LoadConversationResponse>();
        foreach(var convoData in result)
        {
            var RecentMessages = convoData.RecentMessages.Select(m => new ChatData
                (
                    ChatId : _hasher.CreateHashids(m.chatId),
                    ChatMessage : m.chatMessage,
                    TimeStampt : m.timestampt,
                    SenderName : m.senderName,
                    SenderId : _hasher.CreateHashids(request.UserId),
                    RecieverId : _hasher.CreateHashids(convoData.recieverId)
                )
            ).ToList();
            RecentMessages.Reverse();
            response.Add(new LoadConversationResponse
            (
                RoomId : _hasher.CreateHashids(convoData.RoomId),
                LastTimeStampt : RecentMessages.Select(d => d.TimeStampt).LastOrDefault(),
                Chats : RecentMessages
            )
            );
        }
        return Result<List<LoadConversationResponse>>.Success(response);
    }
}