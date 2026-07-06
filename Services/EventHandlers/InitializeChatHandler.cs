using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Features.Chats;
public class InitializeChatCommandHandler : IRequestHandler<InitializeChatCommand, Result<string>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public InitializeChatCommandHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<string>> Handle(InitializeChatCommand command, CancellationToken cancellation)
{
    Result<int> RecieverId = _hasher.DecodeHashids(command.RecieverId);
    if(!RecieverId.IsSuccess)
        return Result<string>.Failure(RecieverId.Error!, RecieverId.StatusCode);
    int? ChatId = await _db.Chatrooms.AsNoTracking()
        .Where(r => r.Participants.Any(p => p.UserId == command.UserId) && 
                    r.Participants.Any(p => p.UserId == RecieverId.Value))
        .Select(r => (int?)r.Id)
        .FirstOrDefaultAsync(cancellation);

    if (ChatId is not null && ChatId.Value != 0)
    {
        return Result<string>.Success(_hasher.CreateHashids(ChatId.Value));
    }
    ChatRoom NewChatRoom = new ChatRoom
    {
        IsGroupChat = false
    };
    await _db.Chatrooms.AddAsync(NewChatRoom, cancellation);
    var Participants = new List<RoomParticipant>
    {
        new RoomParticipant
        {
            Room = NewChatRoom,
            UserId = command.UserId
        },
        new RoomParticipant
        {
            Room = NewChatRoom,
            UserId = RecieverId.Value
        }    
    };
    await _db.participants.AddRangeAsync(Participants, cancellation);
    await _db.SaveChangesAsync(cancellation);
    return Result<string>.Success(_hasher.CreateHashids(NewChatRoom.Id));
}
}