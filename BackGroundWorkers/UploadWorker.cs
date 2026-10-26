using ChatSystem.DataBase;
using MediatR;
using ChatSystem.Models;
using ChatSytstem.Models;
using Microsoft.EntityFrameworkCore;
using ChatSystem.SystemEvents.BackgroundEvents;

namespace ChatSystem.BackgroundServices.UploadWorker;
public class UploadWorker(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while(await timer.WaitForNextTickAsync(cancellationToken))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DbManager>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<UploadWorker>>();
            try
            {
                var pendings = await db.UploadEntries
                    .Where(u => u.Status == UploadStatus.Pending
                        && u.Retries < 5)
                    .OrderBy(e => e.CreatedAt)
                    .Take(100)
                    .ToListAsync(cancellationToken);
                
                if(pendings == null || pendings.Count() == 0) continue;

                foreach(var entry in pendings)
                {
                    UploadFilesAsyncCommand command = new UploadFilesAsyncCommand(entry);
                    await mediator.Send(command, cancellationToken);     
                }
            }
            catch
            {
                
            }
        }
    }
}