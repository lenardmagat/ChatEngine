using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ChatSystem.Features.Chats;
public class OnConnectAutoJoinCommandHandler : IRequestHandler<IOnConnectAutoJoinChat, Result<string>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public OnConnectAutoJoinCommandHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<string>> Handle(IOnConnectAutoJoinChat request, CancellationToken cancellationToken)
    {
        int? UserId = await _db.Users
            .Where(u => u.UserId == request.UserId)
            .Select(d => d.UserId)
            .FirstOrDefaultAsync();
        if(!UserId.HasValue || UserId == 0)
        {
            return Result<string>.Failure("Invalid Token", StatusCodes.Status403Forbidden);
        }
        return Result<string>.Success(_hasher.CreateHashids(UserId.Value));
    }
}