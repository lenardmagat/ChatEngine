using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Services;
public interface IAccountServices
{
    public Task<Result> CreateAccountService(AccountCredentials accountData, CancellationToken cancellation);
    public Task<Result<LoginResponseData>> LoginAccountService(AccountCredentials accountData, CancellationToken cancellation);
    public Task<Result<string>> GetAccountHashIdService(int accountId, CancellationToken cancellation);
}
public class AccountServices : IAccountServices
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public AccountServices(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result> CreateAccountService(AccountCredentials accountData, CancellationToken cancellation)
    {
        if(await _db.Users.AnyAsync(u => u.Username == accountData.Username))
            return Result.Failure("Username alredy exist.", 409);
        User newUser = new User{
            Username =  accountData.Username,
            HashedPassword = _hasher.HashPassword(accountData.password),
            Role = Roles.User,
            Status = true
        };
        await _db.Users.AddAsync(newUser, cancellation);
        await _db.SaveChangesAsync(cancellation);
        return Result.Success();
    }
    public async Task<Result<LoginResponseData>> LoginAccountService(AccountCredentials accountData, CancellationToken cancellation)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == accountData.Username);
        if(user is null) return Result<LoginResponseData>.Failure("Username is not exisiting", 404);
        if(!_hasher.VerifyPassword(accountData.password, user.HashedPassword))
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
    public async Task<Result<string>> GetAccountHashIdService(int accountId, CancellationToken cancellation)
    {
        if(!await _db.Users.Where(u => u.UserId == accountId).AnyAsync())
            return Result<string>.Failure("can't find", 404);
        return Result<string>.Success(_hasher.CreateHashids(accountId, HashContext.User));
    }
}