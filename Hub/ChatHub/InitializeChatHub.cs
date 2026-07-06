using ChatSystem.Extensions;
using ChatSystem.SystemEvents;
using Microsoft.AspNetCore.SignalR;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task InitializeChat(string RecieverId)
    {
        int UserId = Context.User!.GetUserId()!.Value;
        InitializeChatCommand command = new InitializeChatCommand(UserId, RecieverId);
        Result<string> result = await _mediator.Send(command);
        if(!result.IsSuccess)
            await Clients.Caller.SendAsync("Error", new {text = result.Error});
        
        else {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{result.Value}");
            await Clients.Caller.SendAsync("ChatId", new
                {
                    text = result.Value
                }
            );
        }
    }
}