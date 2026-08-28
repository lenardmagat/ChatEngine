using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Inventory;
public partial class InventoryController
{
    [Authorize]
    [HttpGet("ProductsSummary")]
    
    public async Task<IActionResult> GetProdutSummary(
        CancellationToken cancellation
    )
    {
        var userId = User.GetUserId()!.Value;
        GetUserProductSummaryCommand command = new GetUserProductSummaryCommand(userId);
        var result = await _mediator.Send(command, cancellation);
        return result.ToActionResult();
    }
}