using System.Runtime.CompilerServices;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.PipeLine.IsOfferMatch;
public interface IMatchOfferToProduct
{
    string ResourceId {get;}
    OfferTye Status {get;}
}
public class MathOfferToProduct<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMatchOfferToProduct
    where TResponse : Result
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public MathOfferToProduct(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        int ItemId = _hasher.DecodeOrFail(request.ResourceId, HashContext.Product).Value;
        var item = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == ItemId)
            .FirstOrDefaultAsync(cancellationToken);
        if(!item!.IsActive || !item.IsAvailable)
        {
            return CreateFailureResponse("The Item is not available", StatusCodes.Status400BadRequest);
        }
        if(item.Mode == Models.ProductMode.ForSaleOnly && request.Status != OfferTye.Sale)
        {
            return CreateFailureResponse("This item is not for sale.", StatusCodes.Status400BadRequest);
        }
        if(item.Mode == Models.ProductMode.ForTradeOnly && request.Status != OfferTye.Trade)
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