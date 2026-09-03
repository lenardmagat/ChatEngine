using System.Transactions;
using ChatSystem.DataBase;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents.OfferBackgroundEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.OfferMechanisBackgroundWorker;
public class ExpiredOfferHandler(DbManager db, ILogger<ExpiredOfferHandler> logger) : IRequestHandler<ExpiredOfferCommand, Result>
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
                        .SetProperty(p => p.RespondedAt, DateTime.UtcNow),
                        cancellation
                        );
                if(affectedRow == 0)
                {
                    return Result.Failure($"Failed to expire status of the Sale offer item: {command.Value.Itemid}", StatusCodes.Status400BadRequest);
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
                    .SetProperty(p => p.UpdatedA, DateTime.UtcNow),
                    cancellation
                    );
                await db.OutboxEntries.AddAsync(new OutboxEntry
                {
                    EntityId = SaleOfferData!.ItemId,
                    EntityType = DTOs.Documentation.DocumentTarget.Product 
                }, cancellation);
                await db.SaveChangesAsync(cancellation);
                await Transaction.CommitAsync(cancellation);
            }
            else
            {
                affectedRow = await db.TradeOffers
                    .Where(t => t.Id == command.Value.Itemid)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(p => p.Status, Models.TradeOfferStatus.Expired)
                        .SetProperty(p => p.RespondedAt, DateTime.UtcNow),
                        cancellation
                        );
                if(affectedRow == 0)
                {
                    return Result.Failure($"Failed to expire status of the Trade offer item: {command.Value.Itemid}", StatusCodes.Status400BadRequest);
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
                    .SetProperty(p => p.UpdatedA, DateTime.UtcNow),
                    cancellation
                    );
                await db.OutboxEntries.AddAsync(new OutboxEntry
                {
                    EntityId = TradeOfferData!.ItemRequestedId,
                    EntityType = DTOs.Documentation.DocumentTarget.Product 
                },cancellation);
                await db.SaveChangesAsync(cancellation);
                await Transaction.CommitAsync(cancellation);
                
            }
            
            return Result.Success();
        }
        catch(Exception e)
        {
            logger.LogError(e, "Failed to expire offer {ItemId} of type {Type}", command.Value.Itemid, command.Value.Type);
            return Result.Failure("An unexpected error occurred while expiring offer.", StatusCodes.Status500InternalServerError);
        }
    }
}