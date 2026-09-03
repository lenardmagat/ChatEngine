using ChatSystem.core;
using ChatSystem.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Routers.Inventory;
public record Test1DTO(
    int ProductId
);
// Test endpoint disabled for security (CWE-489)
// [ApiController]
// [Route("API/[controller]")]
// public class TestController(DbManager db, IHasher hasher) : ControllerBase
// {
//     [Authorize]
//     [HttpPost("GetProductId")]
//     public async Task<IActionResult> CreateProduct(
//         [FromBody] Test1DTO details,
//         CancellationToken cancellationToken
//     )
//     {
//       return Ok( 
//         hasher.CreateHashids(await db.Products.Where(p => p.Id == details.ProductId).Select(d => d.Id).FirstOrDefaultAsync(), HashContext.Product));
//     }
// }