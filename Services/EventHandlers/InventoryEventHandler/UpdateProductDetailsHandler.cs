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
            if (request.Details.Quantity < 0)
            {
                return Result.Failure("Quantity cannot be negative.", StatusCodes.Status400BadRequest);
            }
            if (request.Details.NewBasePrice < 0)
            {
                return Result.Failure("Price cannot be negative.", StatusCodes.Status400BadRequest);
            }

            var decoded = _hasher.DecodeOrFail(request.Details.ProductId, HashContext.Product);
            if (!decoded.IsSuccess)
            {
                return Result.Failure(decoded.Error!, decoded.StatusCode);
            }

            var productData = await _db.Products
                .Where(p => p.Id == decoded.Value)
                .FirstOrDefaultAsync(cancellationToken);
            if (productData is null)
            {
                return Result.Failure("Product not found.", StatusCodes.Status404NotFound);
            }

            var QuantityDelta = request.Details.IsAddOrRemove == IsAddOrRemove.Add ? request.Details.Quantity : -request.Details.Quantity;
            if (productData.Stock + QuantityDelta < 0)
            {
                return Result.Failure($"Total stock cannot fall below 0.", StatusCodes.Status400BadRequest);
            }
            if (productData.ProductAvailable + QuantityDelta < 0)
            {
                return Result.Failure($"Available stock cannot fall below 0. Reserved items: {productData.ReservedProdcut}.", StatusCodes.Status400BadRequest);
            }

            using var Transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            await _db.Products.Where(d => d.Id == productData.Id).ExecuteUpdateAsync(s => s
                .SetProperty(d => d.Stock, d => d.Stock + QuantityDelta)
                .SetProperty(d => d.ProductAvailable, d => d.ProductAvailable + QuantityDelta)
                .SetProperty(d => d.ProductDescription, request.Details.NewDescription)
                .SetProperty(d => d.ProductName, request.Details.NewName)
                .SetProperty(d => d.BasePrice, request.Details.NewBasePrice)
                .SetProperty(d => d.UpdatedA, DateTime.UtcNow),
                cancellationToken
            );
            await _db.OutboxEntries.AddAsync(
                new OutboxEntry{
                    EntityId = productData.Id, 
                    EntityType = DTOs.Documentation.DocumentTarget.Product
                },
                cancellationToken
            );
            await _db.SaveChangesAsync(cancellationToken);
            await Transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch(Exception e){
            _logger.LogError(e, "An error occurred while updating product {ProductId} for user {UserId}", request.Details.ProductId, request.UserId);
            return Result.Failure("An unexpected error occurred in the server.", StatusCodes.Status500InternalServerError);
        }
    }
}
