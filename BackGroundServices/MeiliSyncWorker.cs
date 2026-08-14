namespace ChatSystem.BackgroundServices.MeiliSync;
public class MeiliSyncWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while(await timer.WaitForNextTickAsync(stoppingToken))
        {
            
        }
    }
}