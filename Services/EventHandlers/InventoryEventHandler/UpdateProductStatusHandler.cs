using System.Diagnostics.Tracing;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Inventory;
public class UpdateProductStatusHandler : IRequestHandler<UpdateProductStatusCommand, Result>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly ILogger<UpdateProductStatusHandler> _logger;
    public UpdateProductStatusHandler(DbManager db, IHasher hasher, ILogger<UpdateProductStatusHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }
    List<TradeOfferStatus> ALLOWEDTRADETOCANCELLED = new List<TradeOfferStatus>{ TradeOfferStatus.Countered, TradeOfferStatus.Proposed, TradeOfferStatus.Accepted};
    List<SaleOfferStatus> ALLOWEDSALETOCANCELLED = new List<SaleOfferStatus>{SaleOfferStatus.Accepted, SaleOfferStatus.Countered, SaleOfferStatus.Proposed};
    public async Task<Result> Handle(UpdateProductStatusCommand details, CancellationToken cancellationToken)
    {
        var decoded = _hasher.DecodeOrFail(details.ResourceId, HashContext.Product);
        if (!decoded.IsSuccess)
        {
            return Result.Failure(decoded.Error!, decoded.StatusCode);
        }
        int productId = decoded.Value;
        var product = await _db.Products.Where(d => d.Id == productId).FirstOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found", StatusCodes.Status404NotFound);
        }
        if(product.IsActive == details.StatusData.NewStatus)
        {
            if(product.IsActive){
                return Result.Failure($"The product is already Active", StatusCodes.Status400BadRequest);
            }
            else
            {
                return Result.Failure($"The product is already In active", StatusCodes.Status400BadRequest);
            }
        }
        using var Transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await _db.Products.Where(d => d.Id == productId).ExecuteUpdateAsync(
            d => d.SetProperty(
                setter => setter.IsActive, details.StatusData.NewStatus)
                , cancellationToken
                );
        if(details.StatusData.NewStatus == false)
        {
            int reservedStockToRelease = 0;
            if (product.Mode == ProductMode.ForSaleOnly || product.Mode == ProductMode.AcceptsBoth)
            {
                var saleOffersToCancel = await _db.SaleOffers
                    .Where(s => s.ItemId == productId && ALLOWEDSALETOCANCELLED.Contains(s.Status))
                    .ToListAsync(cancellationToken);
                reservedStockToRelease += saleOffersToCancel.Sum(s => s.QuantityRequested);
                await _db.SaleOffers
                    .Where(s => s.ItemId == productId && ALLOWEDSALETOCANCELLED.Contains(s.Status))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(setter => setter.Status, SaleOfferStatus.Cancelled)
                        .SetProperty(setter => setter.RespondedAt, DateTime.UtcNow),
                        cancellationToken);
            }
            if (product.Mode == ProductMode.ForTradeOnly || product.Mode == ProductMode.AcceptsBoth)
            {
                var tradeOffersToCancel = await _db.TradeOffers
                    .Where(s => s.ItemRequestedId == productId && ALLOWEDTRADETOCANCELLED.Contains(s.Status))
                    .ToListAsync(cancellationToken);
                reservedStockToRelease += tradeOffersToCancel.Count;
                await _db.TradeOffers
                    .Where(s => s.ItemRequestedId == productId && ALLOWEDTRADETOCANCELLED.Contains(s.Status))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(setter => setter.Status, TradeOfferStatus.Cancelled)
                        .SetProperty(setter => setter.RespondedAt, DateTime.UtcNow),
                        cancellationToken);
            }
            if (reservedStockToRelease > 0)
            {
                await _db.Products.Where(d => d.Id == productId).ExecuteUpdateAsync(
                    d => d
                        .SetProperty(setter => setter.ProductAvailable, setter => setter.ProductAvailable + reservedStockToRelease)
                        .SetProperty(setter => setter.ReservedProdcut, setter => setter.ReservedProdcut - reservedStockToRelease)
                        .SetProperty(setter => setter.UpdatedA, DateTime.UtcNow),
                    cancellationToken
                );
            }
        }
        await _db.OutboxEntries.AddAsync(new OutboxEntry{EntityId = product.Id, EntityType = DTOs.Documentation.DocumentTarget.Product}, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await Transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
   
}
