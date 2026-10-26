using ChatSystem.DataBase;
using ChatSytstem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace ChatSystem.BackgroundServices.UploadWorker;
public class BackgroundWorkerHelper(IServiceScopeFactory serviceScope)
{
    public async Task RecordUploadFailureAsync(int uploadEntryId, CancellationToken cancellationToken)
    {
        await using var scope = serviceScope.CreateAsyncScope();
        var freshDb = scope.ServiceProvider.GetRequiredService<DbManager>();
        
        var entry = await freshDb.UploadEntries.SingleAsync(u => u.UploadEntryId == uploadEntryId);

        entry.Retries += 1;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.Status = entry.Retries >= 5 ?
            UploadStatus.Failed
            : UploadStatus.Pending;
        await freshDb.SaveChangesAsync(cancellationToken);
    }
}