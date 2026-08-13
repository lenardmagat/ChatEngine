using ChatSystem.DTOs.Search;
using ChatSystem.ErrorHandling;
using ChatSystem.Services.Interfaces;
using MediatR;

namespace ChatSystem.SystemEvents.Search;
public static class UnifiedSearch
{
    public record Query(SearchRequest Request) : IRequest<Result<PagedResult<object>>>;

    public class Handler(IEnumerable<ISearchStrategy> strategies) 
        : IRequestHandler<Query, Result<PagedResult<object>>>
    {
        private readonly Dictionary<SearchTarget, ISearchStrategy> _strategyMap = 
            strategies.ToDictionary(s => s.Target);

        public async Task<Result<PagedResult<object>>> Handle(Query query, CancellationToken cancellationToken)
        {
            try{
                var req = query.Request;

                if (!_strategyMap.TryGetValue(req.Target, out var strategy))
                {
                    return Result<PagedResult<object>>.Failure($"No search strategy registered for {req.Target}", StatusCodes.Status400BadRequest);
                }   
                return Result<PagedResult<object>>.Success(await strategy.SearchAsync(req, cancellationToken));
            }catch(Exception e)
            {
                return Result<PagedResult<object>>.Failure(e.Message, e.GetHashCode());
            }
        }
    }
}