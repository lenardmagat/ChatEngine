using ChatSystem.DataBase;
using ChatSystem.Models;
using ChatSystem.SystemEvents.OfferBackgroundEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.BackgroundServices;
public class OfferStatusCheckingWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        List<SaleOfferStatus> AllowedToExpireSaleOffer = new List<SaleOfferStatus>
        {
            SaleOfferStatus.Proposed,
            SaleOfferStatus.Countered
        };
        List<TradeOfferStatus> AllowedToExpireTradeOffer = new List<TradeOfferStatus>
        {
            TradeOfferStatus.Proposed,
            TradeOfferStatus.Countered
        };
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while(await timer.WaitForNextTickAsync(cancellationToken))
        {
            using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DbManager>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OfferStatusCheckingWorker>>();
            try
            {
                var SaleExpiredOffer = await db.SaleOffers
                    .AsNoTracking()
                    .Where(
                        s => DateTime.UtcNow >= s.CreatedAt.AddMinutes(30) && 
                        s.RespondedAt == null && 
                        AllowedToExpireSaleOffer.Contains(s.Status)
                        )
                    .OrderBy(s => s.CreatedAt)
                    .Take(100) 
                    .ToListAsync(cancellationToken);
                var TradeExpiredOffer = await db.TradeOffers
                    .Where(
                        s => DateTime.UtcNow >= s.CreatedAt.AddMinutes(30) && 
                        s.RespondedAt == null && 
                        AllowedToExpireTradeOffer.Contains(s.Status)
                        )
                    .OrderBy(s => s.CreatedAt)
                    .Take(100)
                    .ToListAsync(cancellationToken);
                if(SaleExpiredOffer.Count == 0 && TradeExpiredOffer.Count == 0)
                {
                    continue;
                }

                foreach(var offerExpired in SaleExpiredOffer)
                {
                    try
                    {
                        ExpiredOfferCommand saleCommand = new ExpiredOfferCommand(new ExpiredOfferDTO(offerExpired.Id, DTOs.OfferTye.Sale));
                        var result = await mediator.Send(saleCommand, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            logger.LogError("An error occurred while trying to expire SaleOffer {Id}: {Error}", offerExpired.Id, result.Error);
                        }
                        else
                        {
                            logger.LogInformation("Successfully expired Sale offer Id: {Id}", offerExpired.Id);
                        }
                    }
                    catch(Exception e)
                    {
                        logger.LogCritical(e, "An unexpected error occurred while trying to expire SaleOffer Id: {Id}", offerExpired.Id);
                    }
                }

                foreach(var offerExpired in TradeExpiredOffer)
                {
                    try
                    {
                        ExpiredOfferCommand tradeCommand = new ExpiredOfferCommand(new ExpiredOfferDTO(offerExpired.Id, DTOs.OfferTye.Trade));
                        var result = await mediator.Send(tradeCommand, cancellationToken);
                        if (!result.IsSuccess)
                        {
                            logger.LogError("An error occurred while trying to expire TradeOffer {Id}: {Error}", offerExpired.Id, result.Error);
                        }
                        else
                        {
                            logger.LogInformation("Successfully expired Trade offer Id: {Id}", offerExpired.Id);
                        }
                    }
                    catch(Exception e)
                    {
                        logger.LogCritical(e, "An unexpected error occurred while trying to expire TradeOffer Id: {Id}", offerExpired.Id);
                    }
                }
                
            }
            catch(Exception e)
            {
                logger.LogError(e, $"An unexpected error occured in CheckingOfferStatus BackgroundTask");
            }
        }
    }
}