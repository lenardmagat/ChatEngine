using ChatSystem.DTOs.Search;
using ChatSystem.ErrorHandling;
using ChatSystem.Services.Interfaces;
namespace ChatSystem.EventHandler.Search;
public class UserSearchStrategy(IDynamicSearchService searchService) : ISearchStrategy
{
    public SearchTarget Target => SearchTarget.Users;

    public async Task<PagedResult<object>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        string? roleFilter = request.Filters?.GetValueOrDefault("role");
        PagedResult<UserDto> pagedUsers;
        if(!string.IsNullOrWhiteSpace(roleFilter))
        {
            pagedUsers = await searchService.SearchWithFilterAsync<UserDto>(
                request.Term,
                roleFilter,
                request.Page, 
                request.PageSize, 
                cancellationToken
            );
        }
        else
        {
            pagedUsers = await searchService.SearchAsync<UserDto>(
                request.Term, 
                request.Page, 
                request.PageSize, 
                cancellationToken);
        }
        if(pagedUsers is null)
        {
            return new()
            {
                Items = null,
                TotalCount = 0,
                Page = 0,
                PageSize = 0
            };
        }
        return CastToObjectMapper(pagedUsers);
    }

    private static PagedResult<object> CastToObjectMapper<T>(PagedResult<T> source) where T : class => new()
    {
        Items = source.Items!.Cast<object>().ToList(),
        TotalCount = source.TotalCount,
        Page = source.Page,
        PageSize = source.PageSize
    };
}