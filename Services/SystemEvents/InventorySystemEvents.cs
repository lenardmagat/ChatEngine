using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs.Inventory;
using ChatSystem.ErrorHandling;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.SystemEvents.Inventory;
public interface IOwnedResourceCommand
{
    string ResourceId { get; } 
    int UserId { get; }       
}
public record CreateProductCommand(int UserId, ProductDetailsDTO Details) : IRequest<Result>;
public record UpdateProductCommand : IRequest<Result>, IOwnedResourceCommand
{
    public int UserId {get; set;}
    public UpdateProductDetailsDTO Details {get; set;} = null!;
    public string ResourceId => Details.ProductId;
}
public record UpdateProductStatusCommand  : IRequest<Result>, IOwnedResourceCommand
{
    public int UserId {get; set;}
    public UpdateProductStatusDTO StatusData {get; set;} = null!;
    public string ResourceId => StatusData.ProductId;
}
public record GetUserProductSummaryCommand(int UserId) : IRequest<Result<List<ProductSummaryDto>>>;
public record GetProductDetailsCommand : IRequest<Result<ProductDetailDto>>, IOwnedResourceCommand
{
    public int UserId {get; set;}
    public string ItemId {get; set;} = null!;
    public string ResourceId => ItemId;
}



public class OwnerShipAuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IOwnedResourceCommand
    where TResponse : Result
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public OwnerShipAuthorizationBehaviour(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var DecodedHashId = _hasher.DecodeHashids(request.ResourceId, HashContext.Product);
        if (!DecodedHashId.IsSuccess)
        {
            return CreateFailureResponse("Tampered or Broken Id detected", StatusCodes.Status400BadRequest);
        }
        var product = await _db.Products
            .Where(p => p.Id == DecodedHashId.Value)
            .Select(p => new { p.OwnerUserId })
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            return CreateFailureResponse("Product not found", StatusCodes.Status404NotFound);
        }
        if(product.OwnerUserId != request.UserId)
        {
            return CreateFailureResponse("You do not have permission to access this resource", StatusCodes.Status403Forbidden);
        }
        return await next();
                
    }
    private static TResponse CreateFailureResponse(string message, int statusCode)
{
    if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
    {
        var resultType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result<>)
            .MakeGenericType(resultType)
            .GetMethod("Failure", new[] { typeof(string), typeof(int) });

        return (TResponse)failureMethod!.Invoke(null, new object[] { message, statusCode })!;
    }
    return (TResponse)(object)Result.Failure(message, statusCode);
}
}