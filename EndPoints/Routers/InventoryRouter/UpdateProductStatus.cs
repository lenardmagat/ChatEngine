using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Inventory;
public partial class InventoryController
{
    [HttpPatch("UpdateProductStatus")]
    [Authorize]
    public async Task<IActionResult> UpdateProductStatusRouter(
        [FromBody] UpdateProductStatusDTO updateProductStatusDTO,
        CancellationToken cancellationToken
    )
    {
        
        var Userid = User.GetUserId()!.Value;
        UpdateProductStatusCommand command = new UpdateProductStatusCommand
        {
            UserId = Userid,
            StatusData = updateProductStatusDTO,
        };
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}