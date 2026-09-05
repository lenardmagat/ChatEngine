using ChatSystem.DTOs;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.UnifiedProposedMechanism;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Hubs;
public partial class AppHub
{
    public async Task ProposedOffer(ProposedItemDTO proposedItem)
    {
            var userId = Context.User!.GetUserId()!.Value;
        try
        {
            UnifiedOffer.ProposedCommand proposedCommand = new UnifiedOffer.ProposedCommand
            {
                UserId = userId,
                ProposedData = proposedItem,
            };
            var result = await _mediator.Send(proposedCommand);
            if (!result.IsSuccess)
            {
                await Clients.Caller.SendAsync("ProposedOfferResponse", new
                {
                    Details = result,
                    timestampt = DateTime.UtcNow
                }
                );
            }
            else
            {
                await Clients.Caller.SendAsync("ProposedOfferResponse", result.Value!.MessageData);
                await Clients.Groups($"UsersNotification_{result.Value!.ReceipientId}").SendAsync("NewMessageNotification", result.Value.MessageData);
                await Clients.Groups($"Room_{result.Value!.RoomId}").SendAsync("NewMessage", result.Value.MessageData);
                _logger.LogInformation($"User {userId} Successfully Proposed an item. Detals :{proposedItem}. timestampt: {DateTime.UtcNow}");
            }  
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"An unexpected error occured while handling Proposed Offer endpoint, for user {userId}. Details: {proposedItem}. timestampt{DateTime.UtcNow}");
            await Clients.Caller.SendAsync("RequestError", new {context =  "an Unexpected error occured in our server", statsCode = StatusCodes.Status500InternalServerError, timestampt = DateTime.UtcNow});
        }
    }
}