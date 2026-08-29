using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.PipeLine.IsProductExisting;
public interface IExistingCommandAndMatch
{
    string ResourceId {get;}
    OfferTye Status {get;}
}
public class ProductExistingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IExistingCommandAndMatch
    where TResponse : Result
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public ProductExistingBehaviour(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var Decoded = _hasher.DecodeOrFail(request.ResourceId, HashContext.Product);
        if (!Decoded.IsSuccess)
        {
            return CreateFailureResponse("Tampered or Broken Id detected!", StatusCodes.Status400BadRequest);
        }
        var item = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == Decoded.Value)
            .FirstOrDefaultAsync(cancellationToken);
        if(item is null)
        {
            return CreateFailureResponse("The Item is not Existing", StatusCodes.Status404NotFound);
        }
        if(!item.IsActive || !item.IsAvailable)
        {
            return CreateFailureResponse("The Item is not available", StatusCodes.Status400BadRequest);
        }
        if(item.Mode == ProductMode.ForSaleOnly && request.Status != OfferTye.Sale)
        {
            return CreateFailureResponse("This item is not for sale.", StatusCodes.Status400BadRequest);
        }
        if(item.Mode == ProductMode.ForTradeOnly && request.Status != OfferTye.Trade)
        {
            return CreateFailureResponse("This item is not for sale.", StatusCodes.Status400BadRequest);
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