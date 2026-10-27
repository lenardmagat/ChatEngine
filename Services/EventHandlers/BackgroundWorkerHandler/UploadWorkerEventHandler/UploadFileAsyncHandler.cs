using System.Reflection.Metadata.Ecma335;
using Amazon.S3.Model.Internal.MarshallTransformations;
using ChatSystem.BackgroundServices.UploadWorker;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Documentation;
using ChatSystem.ErrorHandling;
using ChatSystem.Infrastructure;
using ChatSystem.Models;
using ChatSystem.Storage;
using ChatSystem.SystemEvents.BackgroundEvents;
using ChatSytstem.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.BackgroundWorker;
public class UploadFileAsyncHandler(
    DbManager db, 
    ILogger<UploadFileAsyncHandler> logger,
    IFileStorageService fileStorage,
    BackgroundWorkerHelper helper) : IRequestHandler<UploadFilesAsyncCommand, Result>
{
    public async Task<Result> Handle(UploadFilesAsyncCommand command, CancellationToken cancellation)
    {
        using var transaction = await db.Database.BeginTransactionAsync(cancellation);
        var uploadEntry = await db.UploadEntries
            .Where(u => u.UploadEntryId == command.UploadEntry.UploadEntryId)
            .FirstAsync(cancellation);

        try
        {
            PhotoModel photo = new PhotoModel
            {
                PhotoKey = command.UploadEntry.FileKey,
                FileName = command.UploadEntry.FileName
            };
            await db.Photos.AddAsync(photo, cancellation);
            await db.SaveChangesAsync(cancellation);

            switch (command.UploadEntry.Reference)
            {
                case FileReference.Profile:
                    var ProfileUpdateResult = await ChangeProfile(command.UploadEntry.ReferenceId, photo, cancellation);
                    if(!ProfileUpdateResult.IsSuccess){
                        await helper.RecordUploadFailureAsync(command.UploadEntry.UploadEntryId, cancellation);
                        return ProfileUpdateResult;
                    }
                    break;
                
                case FileReference.Product:
                    var ProductPictureUpdateResult = await ChangeProductPicture(command.UploadEntry.ReferenceId, photo, cancellation);
                    if (!ProductPictureUpdateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellation);
                        await helper.RecordUploadFailureAsync(command.UploadEntry.UploadEntryId, cancellation);
                        return ProductPictureUpdateResult;
                    }
                    break;
                
                default:
                    await transaction.RollbackAsync(cancellation);
                    logger.LogError($"Unregistered service. Details: {command.UploadEntry}");
                    return Result.Failure("Unregistered service", StatusCodes.Status404NotFound);
            }

            await using(var input = DirectoryHelper.OpenRead(command.UploadEntry.FileKey))
            {
                var result = await fileStorage.UploadImageAsync(
                    input,
                    photo.PhotoKey,
                    photo.FileName
                );
                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellation);
                    await helper.RecordUploadFailureAsync(uploadEntry.UploadEntryId, cancellation);
                    return result;
                }
                uploadEntry.UpdatedAt = DateTime.UtcNow;
                uploadEntry.Retries += 1;
                uploadEntry.Status = UploadStatus.Completed;
                await db.SaveChangesAsync(cancellation);
            }

            await transaction.CommitAsync(cancellation);
            DirectoryHelper.DeleteAsync(command.UploadEntry.FileKey);
            return Result.Success();

        }
        catch(Exception e)
        {
            await transaction.RollbackAsync(cancellation);
            await helper.RecordUploadFailureAsync(uploadEntry.UploadEntryId, cancellation);
            logger.LogError(e, $"An unexpetected error occured while handling UploadFiles on {command.UploadEntry} entry.");
            return Result.Failure("An unexpected internal server error occured.", StatusCodes.Status500InternalServerError);
        }
    }
    private async Task<Result> ChangeProfile(int Id, PhotoModel photo, CancellationToken cancellationToken)
    {
        int affectedRow = await db.Users.Where(u => u.UserId == Id).ExecuteUpdateAsync(setter => setter
            .SetProperty(p => p.PhotoId, photo.PhotoId),
            cancellationToken
        );
        await db.SaveChangesAsync(cancellationToken);
        if(affectedRow == 0)
        {
            return Result.Failure("Non existence profile.", StatusCodes.Status404NotFound);
        }
        await db.OutboxEntries.AddAsync(new OutboxEntry
            {
                EntityType = DocumentTarget.User,
                EntityId = Id
            },
            cancellationToken
        );
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
    private async Task<Result> ChangeProductPicture(int Id, PhotoModel photo, CancellationToken cancellationToken)
    {
        int affectedRow = await db.Products.Where(p => p.Id == Id).ExecuteUpdateAsync(setter => setter
            .SetProperty(p => p.PhotoId, photo.PhotoId),
            cancellationToken
            );
        await db.SaveChangesAsync(cancellationToken);
        if(affectedRow == 0)
        {
            return Result.Failure("Non existence profile.", StatusCodes.Status404NotFound);
        }

        await db.OutboxEntries.AddAsync(new OutboxEntry
            {
                EntityType = DocumentTarget.Product,
                EntityId = Id
            },
            cancellationToken
        );

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}