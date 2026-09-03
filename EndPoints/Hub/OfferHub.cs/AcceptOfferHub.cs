using ChatSystem.DTOs;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.UnifiedAcceptMechanism;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task AcceptOffer(AcceptItemDTO acceptItem)
    {
        var UserId = Context.User!.GetUserId()!.Value;
        try
        {
            UnifiedAcceptOffer.AcceptOfferCommand command = new UnifiedAcceptOffer.AcceptOfferCommand
            {
                UserId = UserId,
                itemDTO = acceptItem
            };
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("MessageError",new
                {
                    Details = result,
                    timestampt = DateTime.UtcNow
                }
                );
            }
            else
            {
                await Clients.Caller.SendAsync("ProposedOfferResponse", result.Value);
                await Clients.Groups($"UserNotification_{result.Value!.ReceipientId}").SendAsync("NewMessageNotification", result.Value);
                await Clients.Groups($"Room_{result.Value!.RoomId}").SendAsync("NewMessage", result.Value);
                _logger.LogInformation($"User {UserId} Successfully Accept an item. Detals :{acceptItem}. timestampt: {DateTime.UtcNow}");
            } 
        }catch(Exception e)
        {
             _logger.LogError(e, $"An unexpected error occured while handling Accept Offer endpoint, for user {UserId}. Details: {acceptItem}. timestampt{DateTime.UtcNow}");
            await Clients.Caller.SendAsync("RequestError", new {context =  "an Unexpected error occured in our server", statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}