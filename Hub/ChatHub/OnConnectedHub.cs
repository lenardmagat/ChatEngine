using ChatSystem.Extensions;
using ChatSystem.DTOs;
using ChatSystem.SystemEvents;
using Microsoft.AspNetCore.SignalR;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Hubs;
public partial class AppHub
{
    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;
        int UserId = Context.User!.GetUserId()!.Value;
        IOnConnectAutoJoinChat command = new IOnConnectAutoJoinChat(UserId);
        Result<string> result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            await Clients.Caller.SendAsync("MessageError", new
            {
                text = result.Error,
                Timestampt = DateTime.UtcNow
            }
            );
        }
        else
        {
            await Groups.AddToGroupAsync(connectionId, $"UsersNotification_{result.Value}");
        }
        await base.OnConnectedAsync();
    }
}