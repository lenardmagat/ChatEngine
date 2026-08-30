using System.Transactions;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents.OfferBackgroundEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.OfferMechanisBackgroundWorker;
public class ExpiredOfferHandler(DbManager db) : IRequestHandler<ExpiredOfferCommand, Result>
{
    public async Task<Result> Handle(ExpiredOfferCommand command, CancellationToken cancellation)
    {
        using var Transaction = await db.Database.BeginTransactionAsync(cancellation);
        try
        {
            int affectedRow;
            if(command.Value.Type == DTOs.OfferTye.Sale)
            {
                affectedRow = await db.SaleOffers
                    .Where(s => s.Id == command.Value.Itemid)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(p => p.Status, Models.SaleOfferStatus.Expired)
                        .SetProperty(p => p.RespondedAt, DateTime.UtcNow)
                        );
                if(affectedRow == 0)
                {
                    return Result.Failure($"Failed to Expired status of the Sale offer item: {command.Value.Itemid}", StatusCodes.Status400BadRequest);
                }
                var SaleOfferData = await db.SaleOffers
                    .AsNoTracking()
                    .Where(s => s.Id == command.Value.Itemid)
                    .FirstOrDefaultAsync(cancellation);
                await db.Products
                    .Where(p => p.Id == SaleOfferData!.ItemId)
                    .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.ProductAvailable, p => p.ProductAvailable + SaleOfferData!.QuantityRequested)
                    .SetProperty(p => p.ReservedProdcut, p => p.ReservedProdcut - SaleOfferData!.QuantityRequested)
                    );
                await Transaction.CommitAsync(cancellation);
            }
            else
            {
                affectedRow = await db.TradeOffers
                    .Where(t => t.Id == command.Value.Itemid)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(p => p.Status, Models.TradeOfferStatus.Expired)
                        .SetProperty(p => p.RespondedAt, DateTime.UtcNow)
                        );
                if(affectedRow == 0)
                {
                    return Result.Failure($"Failed to Expired status of the Trade offer item: {command.Value.Itemid}", StatusCodes.Status400BadRequest);
                }
                var TradeOfferData = await db.TradeOffers
                    .AsNoTracking()
                    .Where(s => s.Id == command.Value.Itemid)
                    .FirstOrDefaultAsync(cancellation);
                await db.Products
                    .Where(p => p.Id == TradeOfferData!.ItemRequestedId)
                    .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.ProductAvailable, p => p.ProductAvailable + 1)
                    .SetProperty(p => p.ReservedProdcut, p => p.ReservedProdcut - 1)
                    );
                await Transaction.CommitAsync(cancellation);
                
            }
            return Result.Success();
        }
        catch(Exception e)
        {
            return Result.Failure($"Failed to Expired status of the Sale offer item: {command.Value.Itemid}. Details: {e}", StatusCodes.Status400BadRequest);
        }
    }
}