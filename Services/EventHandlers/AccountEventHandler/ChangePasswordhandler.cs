using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ChatSystem.EventHandler.Accounts;
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public ChangePasswordCommandHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == command.UserId, cancellationToken);
        if(user is null)
        {
           return Result.Failure("Invalid Credentials", StatusCodes.Status401Unauthorized); 
        }
        if(!_hasher.VerifyPassword(command.passwordCredentials.OldPassword, user.HashedPassword))
        {
            return Result.Failure("Wrong password", StatusCodes.Status401Unauthorized); 
        }
        string NewHashedPassowrd = _hasher.HashPassword(command.passwordCredentials.NewPassword);
        user.HashedPassword = NewHashedPassowrd;
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(StatusCodes.Status200OK);
    }
}