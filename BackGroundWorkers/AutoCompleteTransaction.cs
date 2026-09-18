using ChatSystem.DataBase;
using ChatSystem.Hubs;
using ChatSystem.SystemEvents.OfferBackgroundEvents;
using MediatR;
using Microsoft.AspNetCore.SignalR;
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
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<AppHub>>(); 
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
                if(SaleOfferCompleted == null || SaleOfferCompleted.Count == 0) continue;
                foreach(int Id in SaleOfferCompleted)
                {
                    AutoCompleteCommand command = new AutoCompleteCommand(new CompletedOfferDTO(Id, DTOs.OfferTye.Sale));
                    var result = await mediator.Send(command, stoppingToken);
                    if (!result.IsSuccess)
                    {
                        logger.LogError(result.Error);
                    }
                    else
                    {
                        await hubContext.Clients.Group($"UsersNotification_{result.Value!.ReceipientId}").SendAsync("NewMessageNotification", result.Value.MessageData);
                        await hubContext.Clients.Group($"UsersNotification_{result.Value!}").SendAsync("NewMessageNotification", result.Value.MessageData);
                        await hubContext.Clients.Groups($"Room_{result.Value!.RoomId}").SendAsync("NewMessage", result.Value.MessageData);
                    }
                }
            }catch(Exception e)
            {
                logger.LogError(e, $"An unexpected error occured in Auto Complete Transaction.");
                continue;
            }
        }
    }
}