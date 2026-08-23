using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Inventory;
public partial class InventoryController
{
    [Authorize]
    [HttpPatch("UpdateProductDetails")]
    public async Task<IActionResult> UpdateProductDetails(
        [FromBody] UpdateProductDetailsDTO details,
        CancellationToken cancellationToken
    )
    {
        int userId = User.GetUserId()!.Value;
        UpdateProductCommand command = new UpdateProductCommand
        {
            UserId = userId,
            Details = details
        };
        Result result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}