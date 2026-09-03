using ChatSystem.core;
using ChatSystem.core.Jwt;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Models;
using ChatSystem.Services.Auth.Jwt;
using ChatSystem.SystemEvents.Accounts;
using ChatSystem.SystemEvents.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Auth;
public class LogInCommandHandler : IRequestHandler<LoginCommand, Result<AuthJWTResponse>>
{
    private readonly DbManager _db;
    private readonly IAuthServices _authServices;
    private readonly IHasher _hasher;
    public LogInCommandHandler(DbManager db, IAuthServices authServices, IHasher hasher)
    {
        _db = db;
        _authServices = authServices;
        _hasher = hasher;
    }
    public async Task<Result<AuthJWTResponse>> Handle(LoginCommand command, CancellationToken cancellation)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == command.Credentials.Username, cancellation);
        if(user is null || !_hasher.VerifyPassword(command.Credentials.password, user.HashedPassword))
        {
            return Result<AuthJWTResponse>.Failure("Invalid username or password.", StatusCodes.Status401Unauthorized);
        }
        if(!user.Status)
        {
            return Result<AuthJWTResponse>.Failure("Account is deactivated or disabled.", StatusCodes.Status401Unauthorized);
        }
        var tokens = await _authServices.IssueTokenAsync(user.UserId);
        if(!tokens.IsSuccess)  return Result<AuthJWTResponse>.Failure(tokens.Error!, tokens.StatusCode);
        return Result<AuthJWTResponse>.Success(tokens.Value!);
    }
}