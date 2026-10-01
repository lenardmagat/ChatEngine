using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Storage;
using ChatSystem.SystemEvents.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Accounts;
public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Result>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly ILogger<CreateAccountHandler> _logger;
    private readonly IFileStorageService _storageService;

    public CreateAccountHandler(DbManager db, IHasher hasher, ILogger<CreateAccountHandler> logger,IFileStorageService fileStorage)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
        _storageService = fileStorage;
    }
    public async Task<Result> Handle(CreateAccountCommand command, CancellationToken cancellation)
    {
        try{
            if (string.IsNullOrWhiteSpace(command.Credentials.password) || command.Credentials.password.Length < 6)
                return Result.Failure("Password must be at least 6 characters long.", StatusCodes.Status400BadRequest);
            if(await _db.Users.AnyAsync(u => u.Username == command.Credentials.Username, cancellation))
                return Result.Failure("Username already exists.", StatusCodes.Status409Conflict);
            using var transaction = await _db.Database.BeginTransactionAsync(cancellation);
            var file = command.Credentials.ProfilePicture;
            var fileName = !string.IsNullOrWhiteSpace(command.Credentials.FileName) 
                ? command.Credentials.FileName 
                : file?.FileName;

            PhotoModel? profilePicture = (file != null && !string.IsNullOrWhiteSpace(fileName)) ? new PhotoModel
            {
                PhotoKey = $"{Guid.NewGuid()}{Path.GetExtension(fileName).ToLowerInvariant()}",
                FileName = fileName
            } : null;

            if (profilePicture != null) 
            {
                await _db.Photos.AddAsync(profilePicture, cancellation);
                await _db.SaveChangesAsync(cancellation);

                var uploadResponse = await _storageService.UploadImageAsync(file!.OpenReadStream(), profilePicture.PhotoKey, fileName!);
                if (!uploadResponse.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellation);
                    return Result.Failure(uploadResponse.Error!, uploadResponse.StatusCode);
                }
            }

            User newUser = new User{
                Username = command.Credentials.Username,
                HashedPassword = _hasher.HashPassword(command.Credentials.password),
                Role = Roles.User,
                Status = true,
                PhotoId = profilePicture?.PhotoId,
                UserProfilePicture = profilePicture
            };


            await _db.Users.AddAsync(newUser, cancellation);
            await _db.SaveChangesAsync(cancellation);
            await _db.OutboxEntries.AddAsync(new OutboxEntry{EntityId = newUser.UserId, EntityType = DTOs.Documentation.DocumentTarget.User}, cancellation);
            await _db.SaveChangesAsync(cancellation);
            await transaction.CommitAsync(cancellation);
            return Result.Success();
        }catch(Exception e)
        {
            _logger.LogError(e, "An error occurred while creating account for username {Username}", command.Credentials.Username);
            return Result.Failure("An unexpected error occurred in the server", StatusCodes.Status500InternalServerError);
        }
    }
}