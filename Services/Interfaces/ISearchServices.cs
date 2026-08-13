using ChatSystem.DTOs.Search;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Services.Interfaces;
public class PagedResult<T>
{
    public List<T>? Items { get; set; }  // The 10 items for the current page
    public long TotalCount { get; set; } // 500 (So frontend can render page numbers [1] [2] [3] ... [50])
    public int Page { get; set; }       // 1
    public int PageSize { get; set; }   // 10
    public PagedResult() { }

    // 4-argument constructor expected by the service
    public PagedResult(List<T> items, long totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
public interface IDynamicSearchService
{
    // Basic search across any DTO
    Task<PagedResult<T>> SearchAsync<T>(
        string query, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default) where T : class;

    // Advanced search accepting raw Meilisearch filter expressions (e.g., "role = 'Admin'")
    Task<PagedResult<T>> SearchWithFilterAsync<T>(
        string query, 
        string meiliFilter, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default) where T : class;
}
public interface ISearchStrategy
{
    SearchTarget Target { get; }
    Task<PagedResult<object>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}