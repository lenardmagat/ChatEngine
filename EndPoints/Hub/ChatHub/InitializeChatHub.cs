using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Chats;
using Microsoft.AspNetCore.SignalR;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task InitializeChat(string? RecieverId, string? ChatId)
    {
        int? userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", new { text = "User is not authenticated" });
            return;
        }
        try
        {
            InitializeChatCommand command = new InitializeChatCommand(userId.Value, RecieverId, ChatId);
            var result = await _mediator.Send(command);
            if(!result.IsSuccess)
                await Clients.Caller.SendAsync("Error", new {text = result.Error});
            
            else {
                if(!result.Value!.IsNew)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{result.Value.RoomId}");
                    await Clients.Caller.SendAsync("ChatId", result);
                }
                else
                {
                    await Clients.Caller.SendAsync("ChatId", new
                        {
                            data = result.Value
                        }
                    );
                }
                _logger.LogInformation("Success initializing chat room for user {UserId}", userId.Value);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to Initialize chat {Target}", ChatId ?? RecieverId);
            await Clients.Caller.SendAsync("Unexpected error occured", new {context = "An unexpected error occurred in our server", statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}