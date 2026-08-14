using ChatSystem.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task ChatDisconnect(string chatId)
    {
        string connectionId = Context.ConnectionId;
        try
        {
            await Groups.RemoveFromGroupAsync(connectionId, chatId);
            _logger.LogInformation($"Successfully disconnected User {Context.User!.GetUserId()!.Value}to {chatId} Chat");
            await Clients.Caller.SendAsync("SuccessToDisconnect", new{UserId = Context.User!.GetUserId()!.Value, chatId});
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Failed to disconnect User from {chatId} Chat room");
            await Clients.Caller.SendAsync("Unexpected error occured", new {context = ex.ToString(), statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}