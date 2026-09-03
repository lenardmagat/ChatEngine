using ChatSystem.Extensions;
using ChatSystem.DTOs;
using ChatSystem.SystemEvents.Chats;
using Microsoft.AspNetCore.SignalR;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.UnifiedChat;
namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task SendMessage(SendMessage request)
    {
        int? userId = Context.User?.GetUserId();
        if (!userId.HasValue)
        {
            await Clients.Caller.SendAsync("MessageError", new { text = "User is not authenticated", Timestampt = DateTime.UtcNow });
            return;
        }
        try
        {
            UnifiedChat.MessageCommand command = new UnifiedChat.MessageCommand(userId.Value, request);
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
                _logger.LogInformation("Success sending message request from {UserId} to {RecipientId}", userId.Value, result.Value!.ReceipientId);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to send a message to {Target}", request.RecieverId ?? request.RoomId);
            await Clients.Caller.SendAsync("Unexpected error occured in our server", new {context = "An unexpected error occurred in our server", statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}