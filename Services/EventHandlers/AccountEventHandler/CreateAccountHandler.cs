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
        try{
            if(await _db.Users.AnyAsync(u => u.Username == command.Credentials.Username))
                return Result.Failure("Username alredy exist.", 409);
            using var transaction = await _db.Database.BeginTransactionAsync(cancellation);
            User newUser = new User{
                Username =  command.Credentials.Username,
                HashedPassword = _hasher.HashPassword(command.Credentials.password),
                Role = Roles.User,
                Status = true
            };
            await _db.Users.AddAsync(newUser, cancellation);
            await _db.SaveChangesAsync(cancellation);
            await _db.OutboxEntries.AddAsync(new OutboxEntry{EntityId = newUser.UserId, EntityType = DTOs.Documentation.DocumentTarget.User}, cancellation);
            await _db.SaveChangesAsync(cancellation);
            await transaction.CommitAsync(cancellation);
            return Result.Success();
        }catch(Exception e)
        {
            return Result.Failure("An unexcepted Error occured in the server", StatusCodes.Status500InternalServerError);
        }
    }
}