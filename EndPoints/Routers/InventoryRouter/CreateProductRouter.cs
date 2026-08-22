using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Routers.Inventory;
public partial class InventoryController
{
    [Authorize]
    [HttpPost("CreateProduct")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] ProductDetails productDetails,
        CancellationToken cancellationToken
    )
    {
        int UserId = User.GetUserId()!.Value;
        CreateProductCommand productCommand = new(UserId, productDetails);
        var result = await _mediator.Send(productCommand, cancellationToken);
        return result.ToActionResult(); 
    }
}