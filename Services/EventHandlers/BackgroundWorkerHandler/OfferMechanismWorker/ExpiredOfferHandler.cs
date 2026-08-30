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
        try
        {
            int affectedRow;
            if(command.Value.Type == DTOs.OfferTye.Sale)
            {
                affectedRow = await db.SaleOffers
                    .Where(s => s.Id == command.Value.Itemid)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(p => p.Status, Models.SaleOfferStatus.Expired)
                        );
                if(affectedRow == 0)
                {
                    return Result.Failure($"Failed to Expired status of the Sale offer item: {command.Value.Itemid}", StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                affectedRow = await db.TradeOffers
                    .Where(t => t.Id == command.Value.Itemid)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(p => p.Status, Models.TradeOfferStatus.Expired)
                        );
                if(affectedRow == 0)
                {
                    return Result.Failure($"Failed to Expired status of the Trade offer item: {command.Value.Itemid}", StatusCodes.Status400BadRequest);
                }
            }
            return Result.Success();
        }
        catch(Exception e)
        {
            return Result.Failure($"Failed to Expired status of the Sale offer item: {command.Value.Itemid}. Details: {e}", StatusCodes.Status400BadRequest);
        }
    }
}