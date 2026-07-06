using ChatSystem.Extensions;
using ChatSystem.SystemEvents;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task LoadChats()
    {
        int UserId = Context.User!.GetUserId()!.Value;
        LoadConversationCommand command = new LoadConversationCommand(UserId);
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("RequestError", new
                {
                    error = result.Error,
                    timestampt = DateTime.UtcNow
                }
            );
        }
        else
        {
            await Clients.Caller.SendAsync("Load Conversation", result.Value);
        }
    }
}