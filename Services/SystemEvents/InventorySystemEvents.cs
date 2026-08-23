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
            return (TResponse)(object)Result.Failure("Tampered or Broken Id detected", StatusCodes.Status401Unauthorized);
        }
        var ownerId = await _db.Products
            .Where(p => p.Id == DecodedHashId.Value)
            .Select(p => p.OwnerUserId)
            .FirstOrDefaultAsync();
        if(ownerId != request.UserId)
        {
            return (TResponse)(object)Result.Failure("You do not own this resource", StatusCodes.Status401Unauthorized);
        }
        return await next();
                
    }
}