using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Accounts;
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Result>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public CreateAccountHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result> Handle(CreateAccountCommand command, CancellationToken cancellation)
    {
        if(await _db.Users.AnyAsync(u => u.Username == command.Credentials.Username))
            return Result.Failure("Username alredy exist.", 409);
        User newUser = new User{
            Username =  command.Credentials.Username,
            HashedPassword = _hasher.HashPassword(command.Credentials.password),
            Role = Roles.User,
            Status = true
        };
        await _db.Users.AddAsync(newUser, cancellation);
        await _db.SaveChangesAsync(cancellation);
        return Result.Success();
    }
}