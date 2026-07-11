using ChatSystem.Extensions;
using ChatSystem.SystemEvents;
using Microsoft.AspNetCore.SignalR;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task InitializeChat(string? RecieverId, string? ChatId)
    {
        try
        {
            int UserId = Context.User!.GetUserId()!.Value;
            InitializeChatCommand command = new InitializeChatCommand(UserId, RecieverId, ChatId);
            var result = await _mediator.Send(command);
            if(!result.IsSuccess)
                await Clients.Caller.SendAsync("Error", new {text = result.Error});
            
            else {
                if(!result.Value!.IsNew)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{result.Value.RoomId}");
                    await Clients.Caller.SendAsync("ChatId", new
                        {
                            text = result.Value
                        }
                    );
                }
                else
                {
                    await Clients.Caller.SendAsync("ChatId", new
                        {
                            data = result.Value
                        }
                    );
                }
                _logger.LogInformation($"Success intializing {result.Value} Chat from user {UserId}");
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Failed to Intialize chat {ChatId ?? RecieverId}");
            await Clients.Caller.SendAsync("Unexpected error occured", new {context = ex.ToString(), statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}