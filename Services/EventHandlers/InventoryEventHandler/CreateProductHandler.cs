using System.Runtime.CompilerServices;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Inventory;
public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result>
{
    DbManager _db;
    ILogger<CreateProductHandler> _logger;
    public CreateProductHandler(DbManager db, ILogger<CreateProductHandler> logger)
    {
        _db = db;
        _logger =logger;
    }
    public async Task<Result> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {   try{
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
                    Mode = command.Details.Mode
                    };
            using var Transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            await _db.Products.AddAsync(NewProduct, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            if(command.Details.Mode != ProductMode.DeclineBoth)
            {
                await _db.OutboxEntries.AddAsync(
                new OutboxEntry
                    {
                        EntityType = DTOs.Documentation.DocumentTarget.Product,
                        EntityId = NewProduct.Id
                    },
                    cancellationToken
                );
                }
            await _db.SaveChangesAsync(cancellationToken);
            await Transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }catch(Exception e)
        {
            _logger.LogCritical(e, $"Unexpected error occured while processing request to Create Product of user{command.UserId}. Details:{command.Details}");
            return Result.Failure("Unexpected error occured in server", StatusCodes.Status500InternalServerError);
        }
        
    }
}