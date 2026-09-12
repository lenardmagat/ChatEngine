using ChatSystem.DTOs;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.UnifiedDeclineMechanism;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Hubs;

public partial class AppHub
{
    public async Task DeclineOffer(DeclineItemDTO declineItem)
    {
        var UserId = Context.User!.GetUserId()!.Value;
        try
        {
            UnifiedDeclineOffer.DeclineOfferCommand command = new UnifiedDeclineOffer.DeclineOfferCommand
            {
                UserId = UserId,
                ItemDTO = declineItem
            };
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("MessageError", new
                {
                    Details = result,
                    timestampt = DateTime.UtcNow
                });
            }
            else
            {
                await Clients.Caller.SendAsync("DeclineOfferResponse", result.Value!.MessageData);
                await Clients.Groups($"UsersNotification_{result.Value!.ReceipientId}").SendAsync("NewMessageNotification", result.Value.MessageData);
                await Clients.Groups($"Room_{result.Value!.RoomId}").SendAsync("NewMessage", result.Value.MessageData);
                _logger.LogInformation($"User {UserId} Successfully Declined an item. Details: {declineItem}. timestampt: {DateTime.UtcNow}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"An unexpected error occured while handling Decline Offer endpoint, for user {UserId}. Details: {declineItem}. timestampt: {DateTime.UtcNow}");
            await Clients.Caller.SendAsync("RequestError", new { context = "an Unexpected error occured in our server", statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow });
        }
    }
}
