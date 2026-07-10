using ChatSystem.Extensions;
using ChatSystem.DTOs;
using ChatSystem.SystemEvents;
using Microsoft.AspNetCore.SignalR;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task DirectMessage(string? ChatId, string Message, string? RecipiendId)
    {
        int UserId = Context.User!.GetUserId()!.Value;
        SendMessageCommand command = new SendMessageCommand(UserId, new SendMessage(ChatId, Message, RecipiendId));
        Result<MessageResponseDTO> result = await _mediator.Send(command);
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
            await Clients.Caller.SendAsync("NewMessage", result.Value);
            await Clients.Group($"UsersNotification_{result.Value!.ReceipientId}").SendAsync("NewMessageNotification", result.Value);
            await Clients.OthersInGroup($"Room_{result.Value!.RoomId}").SendAsync("NewMessage", result.Value);
        }
    }
}