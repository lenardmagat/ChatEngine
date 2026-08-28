using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;

namespace ChatSystem.EventHandler.Inventory;
public class GetProductDetailsHandler : IRequestHandler<GetProductDetailsCommand, Result<ProductDetailDto>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly ILogger<GetProductDetailsHandler> _logger;
    public GetProductDetailsHandler(DbManager db, IHasher hasher, ILogger<GetProductDetailsHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger; 
    }
    public async Task<Result<ProductDetailDto>> Handle(GetProductDetailsCommand command, CancellationToken cancellationToken)
    {
        try{
            var productId = _hasher.DecodeHashids(command.ItemId, HashContext.Product).Value;
            var product = await _db.Products.AsNoTracking().Where(p => p.Id == productId).FirstOrDefaultAsync(cancellationToken);
            return Result<ProductDetailDto>.Success(
                new ProductDetailDto(
                    _hasher.CreateHashids(product!.Id, HashContext.Product),
                    product.ProductName,
                    product.ProductDescription,
                    product.BasePrice,
                    product.Stock,
                    product.ProductAvailable,
                    product.ReservedProdcut,
                    product.Mode,
                    product.IsAvailable,
                    product.IsActive,
                    product.CreatedAt,
                    product.UpdatedA
                )
            );
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"error occured while handling GetProductDetailsHandler. Details=> UserId: {command.UserId}, ProductId: {command.ItemId}");
            return Result<ProductDetailDto>.Failure("An unexpected error occured in internal server", StatusCodes.Status500InternalServerError);
        }
    }
}