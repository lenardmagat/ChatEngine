using ChatSystem.DataBase;
using ChatSystem.DTOs.Documentation;
using ChatSystem.SystemEvents.Documentation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.BackgroundServices.MeiliSync;
public class MeiliSyncWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while(await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope =  scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DbManager>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MeiliSyncWorker>>();
            try{
            
            var pending = await db.OutboxEntries
                .Where(e => e.ProcessedAt ==null)
                .OrderBy(e => e.CreatedAt)
                .Take(100)
                .ToListAsync();
            if(pending is null){
                logger.LogInformation("no outbox entry to process");
                continue;
            }
            foreach(var entry in pending)
            {
                DocumentRequest request = new DocumentRequest(entry.EntityId.ToString(), entry.EntityType);
                var result = await mediator.Send(new UnifiedDocument.DocumentationCommand(request), stoppingToken);
                if(result.IsSuccess) entry.ProcessedAt = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
            }catch(Exception e)
            {
                logger.LogError(e.Message);
            }
        }
    }
}