using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChatSystem.Routers.Inventory;
public partial class InventoryController
{
    [Authorize]
    [HttpPost("CreateProduct")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] ProductDetailsDTO productDetails,
        CancellationToken cancellationToken
    )
    {
        int UserId = User.GetUserId()!.Value;
        CreateProductCommand productCommand = new(UserId, productDetails);
        var result = await _mediator.Send(productCommand, cancellationToken);
        return result.ToActionResult(); 
    }
}