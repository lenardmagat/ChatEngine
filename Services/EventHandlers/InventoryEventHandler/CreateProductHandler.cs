using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Transactions;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Infrastructure;
using ChatSystem.Models;
using ChatSystem.Storage;
using ChatSystem.SystemEvents.Inventory;
using ChatSytstem.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Inventory;
public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result>
{
    DbManager _db;
    ILogger<CreateProductHandler> _logger;
    IFileStorageService _fileStorage;
    public CreateProductHandler(DbManager db, ILogger<CreateProductHandler> logger, IFileStorageService fileStorageService)
    {
        _db = db;
        _logger =logger;
        _fileStorage = fileStorageService;
    }
    public async Task<Result> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {   
        using var Transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try{
            User? Owner = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == command.UserId);
            if(Owner is null) return Result.Failure("Credential is not existing", StatusCodes.Status400BadRequest);

            var NewProduct = new Product{
                    OwnerUserId = Owner.UserId,
                    ProductName = command.Details.Name, 
                    ProductDescription = command.Details.Description,
                    BasePrice = command.Details.Baseprice,
                    Stock = command.Details.Stock,
                    ProductAvailable = command.Details.Stock,
                    ReservedProdcut = 0,
                    Mode = command.Details.Mode,
                    PhotoId = null,
                    ProductPicture = null
                    };
            
            await _db.Products.AddAsync(NewProduct, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await _db.OutboxEntries.AddAsync(
                new OutboxEntry
                {
                    EntityType = DTOs.Documentation.DocumentTarget.Product,
                    EntityId = NewProduct.Id
                },
                cancellationToken
            );      

            if(command.Details.FileName != null && command.Details.ProductIMG != null)
            {
                if (!_fileStorage.AllowedImageExtension(command.Details.ProductIMG.FileName.ToLowerInvariant()))
                {
                    return Result.Failure("Not allowed media type.", StatusCodes.Status415UnsupportedMediaType);
                }

                string fileKey = await DirectoryHelper.SaveAsync(command.Details.ProductIMG, cancellationToken);
                await _db.UploadEntries.AddAsync(
                    new UploadEntry
                    {
                        FileKey = fileKey,
                        FileName = command.Details.ProductIMG.FileName,
                        Status = UploadStatus.Pending,
                        Retries = 0,
                        Reference = FileReference.Product,
                        ReferenceId = NewProduct.Id
                    },
                    cancellationToken
                );
            }
            await _db.SaveChangesAsync(cancellationToken);
            await Transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }catch(Exception e)
        {
            await Transaction.RollbackAsync(cancellationToken);
            _logger.LogCritical(e, $"Unexpected error occured while processing request to Create Product of user{command.UserId}. Details:{command.Details}");
            return Result.Failure("Unexpected error occured in server", StatusCodes.Status500InternalServerError);
        }
        
    }
}