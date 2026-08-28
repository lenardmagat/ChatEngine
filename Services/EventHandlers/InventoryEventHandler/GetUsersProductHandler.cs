using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.Inventory;
public class GetProductSummaryHandler : IRequestHandler<GetUserProductSummaryCommand, Result<List<ProductSummaryDto>>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly ILogger<GetProductSummaryHandler> _logger;
    public GetProductSummaryHandler(DbManager db, IHasher hasher, ILogger<GetProductSummaryHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }
    public async Task<Result<List<ProductSummaryDto>>> Handle(GetUserProductSummaryCommand request, CancellationToken cancellation)
    {
        try{
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if(user is null)
            {
                return Result<List<ProductSummaryDto>>.Failure("Tampered or Broken Id detected", StatusCodes.Status401Unauthorized);
            }
            var UserProducts = await _db.Products
                .AsNoTracking()
                .Where(Product =>
                    Product.OwnerUserId == user.UserId)
                .Select(productDetails => new{
                    productDetails.Id,
                    productDetails.ProductName,
                    productDetails.BasePrice,
                    productDetails.IsAvailable,
                    productDetails.Mode
                } 
                    )
                .ToListAsync(cancellation);
            List<ProductSummaryDto> products = new List<ProductSummaryDto>();
            foreach(var product in UserProducts)
            {
                products.Add(new ProductSummaryDto(
                    _hasher.CreateHashids(product.Id, HashContext.Product),
                    product.ProductName,
                    product.BasePrice,
                    product.IsAvailable,
                    product.Mode
                    )
                );
            }
            return Result<List<ProductSummaryDto>>.Success(products);
            }
        catch(Exception e){
            _logger.LogError(e, $"An error occured while handling GetUserProductSummary User {request.UserId}.");
            return Result<List<ProductSummaryDto>>.Failure("An internal server errror occured.", StatusCodes.Status500InternalServerError);
        }
    }
}