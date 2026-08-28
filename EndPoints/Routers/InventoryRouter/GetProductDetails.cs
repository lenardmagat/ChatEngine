using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Inventory;
public partial class InventoryController
{
    [Authorize]
    [HttpGet("ProductDetails/{productId}")]
    public async Task<IActionResult> GetProductDetailsEndpoint(
        [FromRoute] string productId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!.Value;
        GetProductDetailsCommand command = new GetProductDetailsCommand
        {
            UserId = userId,
            ItemId = productId
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}