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
        int productId = _hasher.DecodeHashids(details.ResourceId, HashContext.Product).Value;
        var product = await _db.Products.Where(d => d.Id == productId).FirstOrDefaultAsync(cancellationToken);
        using var Transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await _db.Products.Where(d => d.Id == productId).ExecuteUpdateAsync(d => d.SetProperty(setter => setter.IsAvailable, details.StatusData.NewStatus), cancellationToken);
        if(details.StatusData.NewStatus == false)
        {
            switch (product!.Mode)
            {
                case ProductMode.ForSaleOnly:
                    await _db.SaleOffers
                        .Where(s => s.ItemId == productId && ALLOWEDSALETOCANCELLED
                            .Contains(s.Status))
                        .ExecuteUpdateAsync(s => s.SetProperty(setter => setter
                            .Status, SaleOfferStatus.Cancelled));
                    break;
                case ProductMode.ForTradeOnly:
                    await _db.TradeOffers
                        .Where(s => s.ItemRequestedId == productId && ALLOWEDTRADETOCANCELLED
                            .Contains(s.Status))
                        .ExecuteUpdateAsync(s => s.SetProperty(setter => setter
                            .Status, TradeOfferStatus.Cancelled));
                    break;
                case ProductMode.AcceptsBoth:
                    await _db.SaleOffers
                        .Where(s => s.ItemId == productId && ALLOWEDSALETOCANCELLED
                            .Contains(s.Status))
                        .ExecuteUpdateAsync(s => s.SetProperty(setter => setter
                            .Status, SaleOfferStatus.Cancelled));
                    await _db.TradeOffers
                        .Where(s => s.ItemRequestedId == productId && ALLOWEDTRADETOCANCELLED
                            .Contains(s.Status))
                        .ExecuteUpdateAsync(s => s.SetProperty(setter => setter
                            .Status, TradeOfferStatus.Cancelled));
                    break;
            }
        }
        await _db.OutboxEntries.AddAsync(new OutboxEntry{EntityId = product!.Id, EntityType = DTOs.Documentation.DocumentTarget.Product});
        await Transaction.CommitAsync();
        return Result.Success();
    }
   
}
