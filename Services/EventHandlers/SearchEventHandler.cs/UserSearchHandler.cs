using ChatSystem.core;
using ChatSystem.DTOs.Documentation;
using ChatSystem.DTOs.Search;
using ChatSystem.Services.Interfaces;
namespace ChatSystem.EventHandler.Search;
public class UserSearchStrategy(IDynamicSearchService searchService, IHasher hasher) : ISearchStrategy
{
    public SearchTarget Target => SearchTarget.Users;

    public async Task<PagedResult<object>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        string? roleFilter = request.Filters?.GetValueOrDefault("role");
        PagedResult<UserDocumentation> rawpagedUsers;
        if(!string.IsNullOrWhiteSpace(roleFilter))
        {
            rawpagedUsers = await searchService.SearchWithFilterAsync<UserDocumentation>(
                request.Term,
                $"role = '{roleFilter}'",
                request.Page, 
                request.PageSize, 
                cancellationToken
            );
        }
        else
        {
            rawpagedUsers = await searchService.SearchAsync<UserDocumentation>(
                request.Term, 
                request.Page, 
                request.PageSize, 
                cancellationToken);
        }
        if(rawpagedUsers is null)
        {
            return new()
            {
                Items = null,
                TotalCount = 0,
                Page = 0,
                PageSize = 0
            };
        }
        PagedResult<UserSearchDTOResponse> pagedResult = rawpagedUsers
            .Select(user => new UserSearchDTOResponse
                (
                    hasher.CreateHashids(int.Parse(user.id), HashContext.User),
                    user.Username
                )
            );
        return pagedResult.CastToObjectMapper();
    }
}