using System.Net.Http.Headers;
using System.Net.Mail;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Inventory;
public class UpdateProductDetailsHandler : IRequestHandler<UpdateProductCommand, Result>
{
    DbManager _db;
    IHasher _hasher;
    ILogger<UpdateProductDetailsHandler> _logger;
    public UpdateProductDetailsHandler(DbManager db, IHasher hasher, ILogger<UpdateProductDetailsHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try{
            var QuantityDelta = request.Details.IsAddOrRemove == IsAddOrRemove.Add ? request.Details.Quantity : -request.Details.Quantity;
            var productData = await _db.Products
                .Where(p => p.Id == _hasher.DecodeHashids(request.Details.ProductId, HashContext.Product).Value!)
                .FirstOrDefaultAsync();
            if(productData!.Stock + QuantityDelta < 0)
            {
                return Result.Failure($"You cannot remove {request.Details.Quantity}. Add more quantity or Decline offer to remove product quantity.", StatusCodes.Status400BadRequest);
            }
            using var Transaction = await _db.Database.BeginTransactionAsync();
            await _db.Products.Where(d => d.Id == productData.Id).ExecuteUpdateAsync(s => s
                .SetProperty(d => d.Stock, d => d.Stock + QuantityDelta)
                .SetProperty(d => d.ProductAvailable, d => d.ProductAvailable + QuantityDelta)
                .SetProperty(d => d.ProductDescription, request.Details.NewDescription)
                .SetProperty(d => d.ProductName, request.Details.NewName)
                .SetProperty(d => d.BasePrice, request.Details.NewBasePrice)
            );
            await _db.OutboxEntries.AddAsync(
                new OutboxEntry{
                    EntityId = productData.Id, 
                    EntityType = DTOs.Documentation.DocumentTarget.Product
                    }
                );
            await _db.SaveChangesAsync();
            await Transaction.CommitAsync();
            return Result.Success();
            }
        catch(Exception e){
            _logger.LogCritical(e, $"An critical bug occured while processing update on Product. Details:{request.Details}");
            return Result.Failure("An Unexepected error occcured in the server", StatusCodes.Status500InternalServerError);
        }
    }
}
