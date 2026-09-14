using ChatSystem.DataBase;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.BackgroundServices;
public class AutoCompleteTransactionWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while(await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DbManager>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AutoCompleteTransactionWorker>>();
            try
            {
                var SaleOfferCompleted = await db.SaleOffers
                    .AsNoTracking()
                    .Where(o => 
                        o.Status == Models.SaleOfferStatus.Accepted && 
                        DateTime.UtcNow >= o.RespondedAt!.Value.AddMinutes(30)
                        )
                    .Select(o => o.Id)
                    .Take(100)
                    .ToListAsync(stoppingToken);
                
            }catch(Exception e)
            {
                
            }
        }
    }
}