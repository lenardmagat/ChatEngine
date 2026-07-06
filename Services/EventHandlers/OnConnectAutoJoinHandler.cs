using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ChatSystem.Features.Chats;
public class OnConnectAutoJoinCommandHandler : IRequestHandler<IOnConnectAutoJoinChat, Result<List<string>>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public OnConnectAutoJoinCommandHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<List<string>>> Handle(IOnConnectAutoJoinChat request, CancellationToken cancellationToken)
    {
        var ChatIds = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserId == request.UserId)
            .Select(u => new 
            {
                RoomIds = _db.participants
                    .Where(p => p.UserId == request.UserId)
                    .Select(p => p.RoomId)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        if(ChatIds is null)
        {
            return Result<List<string>>.Failure("Invalid Credential", StatusCodes.Status401Unauthorized);
        }
        
        List<string> HashIdList = ChatIds.RoomIds.Select(c => _hasher.CreateHashids(c)).ToList();
        return Result<List<string>>.Success(HashIdList);
    }
}