using ChatSystem.DTOs.Search;
using ChatSystem.DTOs.Documentation;
namespace ChatSystem.Services.Interfaces;
public class PagedResult<T>
{
    public List<T>? Items { get; set; }
    public long TotalCount { get; set; } 
    public int Page { get; set; } 
    public int PageSize { get; set; } 
    public PagedResult() { }
    public PagedResult(List<T> items, long totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
    public PagedResult<TResult> Select<TResult>(Func<T, TResult> map)
    {
        return new PagedResult<TResult>(
            Items?.Select(map).ToList() ?? new List<TResult>(),
            TotalCount,
            Page,
            PageSize
        );
    }
}
public static class PageResultExtensions
{
    public static PagedResult<Object> CastToObjectMapper<T> (this PagedResult<T> source) where T: class
    {
        return new(){
            Items = source.Items!.Cast<object>().ToList(),
            TotalCount = source.TotalCount,
            Page = source.Page,
            PageSize = source.PageSize
        };
    }
}
public interface IDynamicSearchService
{
    Task<PagedResult<T>> SearchAsync<T>(
        string query, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default) where T : class;
    Task<PagedResult<T>> SearchWithFilterAsync<T>(
        string query, 
        string meiliFilter, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default) where T : class;
    
    Task IndexAsync<T>(T document, CancellationToken cancellationToken = default) where T : class;
    Task DeleteFromIndexAsync<T>(string documentId, CancellationToken cancellationToken = default) where T : class;
}
public interface ISearchStrategy
{
    SearchTarget Target { get; }
    Task<PagedResult<object>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
public interface IDocumentStrategy
{
    DocumentTarget Target {get; }
    Task DocumentAsync(DocumentRequest request, CancellationToken cancellation = default);
}