using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace ChatSystem.EventHandler.Accounts;
public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseData>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public LoginHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }   
    public async Task<Result<LoginResponseData>> Handle(LoginCommand command, CancellationToken cancellation)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == command.Credentials.Username);
        if(user is null) return Result<LoginResponseData>.Failure("Username is not exisiting", 404);
        if(!_hasher.VerifyPassword(command.Credentials.password, user.HashedPassword))
        {
            return Result<LoginResponseData>.Failure("Wrong Password.", 404);
        }
        return Result<LoginResponseData>.Success(new LoginResponseData
            (
                JwtToken: _hasher.CreateToken(user.UserId),
                timestamp: DateTime.UtcNow
            )   
        );
    }
}