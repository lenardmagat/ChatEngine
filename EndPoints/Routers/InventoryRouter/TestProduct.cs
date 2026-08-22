using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling.Extension;
using ChatSystem.Extensions;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Routers.Inventory;
public record Test1DTO(
    int ProductId
);
[ApiController]
[Route("API/[controller]")]
public class TestController(DbManager db, IHasher hasher) : ControllerBase
{
    [Authorize]
    [HttpPost("GetProductId")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] Test1DTO details,
        CancellationToken cancellationToken
    )
    {
      return Ok( 
        hasher.CreateHashids(await db.Products.Where(p => p.Id == details.ProductId).Select(d => d.Id).FirstOrDefaultAsync(), HashContext.Product));
    }
}