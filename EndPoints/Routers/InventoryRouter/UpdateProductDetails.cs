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
        [FromBody] UpdateProductDetails details,
        CancellationToken cancellationToken
    )
    {
        int UserId = User.GetUserId()!.Value;
        UpdateProductCommand command = new UpdateProductCommand(UserId, details);
        Result result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}