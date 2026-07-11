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
        try
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
                _logger.LogInformation($"Success sending message request from {UserId} to {result.Value!.ReceipientId}");
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Failed to send a direct message to  {ChatId ?? RecipiendId}");
            await Clients.Caller.SendAsync("Unexpected error occured", new {context = ex.ToString(), statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}