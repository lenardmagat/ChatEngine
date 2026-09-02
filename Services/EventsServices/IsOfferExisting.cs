using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.PipeLine.IsOfferExisting;
public interface IOfferExist
{
    string ParentOfferId {get;}
    OfferTye Status {get;}
}
public class OfferCheck<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IOfferExist
    where TResponse : Result
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public OfferCheck(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellation)
    {
        Result<int> Decoded;
        if(request.Status == OfferTye.Sale)
        {
            Decoded = _hasher.DecodeOrFail(request.ParentOfferId, HashContext.SaleOffer);
        }
        else
        {
            Decoded = _hasher.DecodeOrFail(request.ParentOfferId, HashContext.TradeOffer);
        }
        if (!Decoded.IsSuccess)
        {
            return CreateFailureResponse(Decoded.Error!, Decoded.StatusCode);
        }
        if(request.Status == OfferTye.Sale)
        {
            var offer= await _db.SaleOffers
                .AsNoTracking()
                .Where(o => o.Id == Decoded.Value)
                .FirstOrDefaultAsync(cancellation);
            if(offer is null)
            {
                return CreateFailureResponse("The offer is not existing", StatusCodes.Status400BadRequest);
            }
           return await next();
        }
        else
        {
            var offer= await _db.TradeOffers
                .AsNoTracking()
                .Where(o => o.Id == Decoded.Value)
                .FirstOrDefaultAsync(cancellation);
            if(offer is null)
            {
                return CreateFailureResponse("The offer is not existing", StatusCodes.Status400BadRequest);
            }
           return await next();
        }

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