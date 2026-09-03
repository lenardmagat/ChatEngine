using ChatSystem.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task ChatDisconnect(string chatId)
    {
        string connectionId = Context.ConnectionId;
        int? userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            await Clients.Caller.SendAsync("RequestError", new { context = "User is not authenticated", statsCode = StatusCodes.Status401Unauthorized, timestampt = DateTime.UtcNow });
            return;
        }
        try
        {
            await Groups.RemoveFromGroupAsync(connectionId, chatId);
            _logger.LogInformation("Successfully disconnected User {UserId} from {ChatId} Chat", userId.Value, chatId);
            await Clients.Caller.SendAsync("SuccessToDisconnect", new { UserId = userId.Value, chatId });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect User {UserId} from {ChatId} Chat room", userId.Value, chatId);
            await Clients.Caller.SendAsync("Unexpected error occured", new { context = "An unexpected error occurred in our server", statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow });
        }
    }
}