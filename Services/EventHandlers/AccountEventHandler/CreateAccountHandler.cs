using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Infrastructure;
using ChatSystem.Models;
using ChatSystem.Storage;
using ChatSystem.SystemEvents.Accounts;
using ChatSytstem.Models;
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
            using var transaction = await _db.Database.BeginTransactionAsync(cancellation);
        try{
            if (string.IsNullOrWhiteSpace(command.Credentials.password) || command.Credentials.password.Length < 6)
                return Result.Failure("Password must be at least 6 characters long.", StatusCodes.Status400BadRequest);
            if(await _db.Users.AnyAsync(u => u.Username == command.Credentials.Username, cancellation))
                return Result.Failure("Username already exists.", StatusCodes.Status409Conflict);

            User newUser = new User{
                Username = command.Credentials.Username,
                HashedPassword = _hasher.HashPassword(command.Credentials.password),
                Role = Roles.User,
                Status = true,
                PhotoId = null,
                UserProfilePicture = null
            };

            await _db.Users.AddAsync(newUser, cancellation);
            await _db.SaveChangesAsync(cancellation);
            await _db.OutboxEntries.AddAsync(new OutboxEntry{EntityId = newUser.UserId, EntityType = DTOs.Documentation.DocumentTarget.User}, cancellation);
            await _db.SaveChangesAsync(cancellation);

            if(command.Credentials.FileName != null && command.Credentials.ProfilePicture != null)
            {
                if (!_storageService.AllowedImageExtension(command.Credentials.ProfilePicture.FileName))
                {
                    await transaction.RollbackAsync(cancellation);
                    return Result.Failure("Not allowed media type.", StatusCodes.Status415UnsupportedMediaType);
                }

                string fileKey = await DirectoryHelper.SaveAsync(command.Credentials.ProfilePicture, cancellation);
                await _db.UploadEntries.AddAsync(new UploadEntry
                    {
                        FileKey = fileKey,
                        FileName = command.Credentials.ProfilePicture.FileName,
                        Status = UploadStatus.Pending,
                        Retries = 0,
                        Reference = FileReference.Profile,
                        ReferenceId = newUser.UserId
                    },
                    cancellation
                );
                await _db.SaveChangesAsync(cancellation);
            }

            await transaction.CommitAsync(cancellation);
            return Result.Success();
        }catch(Exception e)
        {
            await transaction.RollbackAsync(cancellation);
            _logger.LogError(e, "An error occurred while creating account for username {Username}", command.Credentials.Username);
            return Result.Failure("An unexpected error occurred in the server", StatusCodes.Status500InternalServerError);
        }
    }
}