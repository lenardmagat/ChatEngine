using Meilisearch;
using ChatSystem.Services.Interfaces;
using ChatSystem.DTOs.Documentation;
namespace ChatSystem.Services;
public class DynamicMeiliSearchService : IDynamicSearchService
{
    private readonly MeilisearchClient _client;
    public DynamicMeiliSearchService(MeilisearchClient client)
    {
        _client = client;
    }

    public Task<PagedResult<T>> SearchAsync<T>(
        string query, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default) where T : class
    {
        return SearchWithFilterAsync<T>(query, meiliFilter: null, page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<T>> SearchWithFilterAsync<T>(
        string query, 
        string? meiliFilter, 
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default) where T : class
    {
        string indexName = typeof(T).Name.ToLower();
        var index = _client.Index(indexName);
        var searchQuery = new SearchQuery
        {
            Limit = pageSize,
            Offset = (page - 1) * pageSize,
            Filter = meiliFilter
        };
        var rawResult = await index.SearchAsync<T>(query, searchQuery, cancellationToken);
        if (rawResult is SearchResult<T> searchResult)
        {
            return new PagedResult<T>
            {
                Items = searchResult.Hits.ToList(),
                TotalCount = searchResult.EstimatedTotalHits,
                Page = page,
                PageSize = pageSize
            };
        }

        return new PagedResult<T> { Items = new List<T>(), TotalCount = 0, Page = page, PageSize = pageSize };
    }

    public async Task IndexAsync<T>(T document, CancellationToken CancellationToken = default) where T : class
    {
        string IndexName = typeof(T).Name.ToLower();
        try
        {   
            await _client.GetIndexAsync(IndexName, CancellationToken);
        }
        catch (MeilisearchApiError ex) when (ex.Code == "index_not_found")
        {
            var createTask = await _client.CreateIndexAsync(IndexName, primaryKey: "id", cancellationToken: CancellationToken);
            await _client.WaitForTaskAsync(createTask.TaskUid, cancellationToken: CancellationToken);
            if(typeof(T) == typeof(ProductDocumentation))
            {
                var newIndex = _client.Index(IndexName);
                var fitlerTask = await newIndex.UpdateFilterableAttributesAsync(
                    new[]
                    {
                        "isActive", "isAvailable", "productStatus"
                    },
                    CancellationToken
                );
                await _client.WaitForTaskAsync(fitlerTask.TaskUid, cancellationToken: CancellationToken);
            }
        }
        var index = _client.Index(IndexName);
        var task = await index.AddDocumentsAsync(new[] {document}, cancellationToken: CancellationToken);
        await index.WaitForTaskAsync(task.TaskUid, cancellationToken: CancellationToken);
    } 
    public async Task DeleteFromIndexAsync<T>(string documentId, CancellationToken cancellationToken = default) where T : class
    {
        string IndexName = typeof(T).Name.ToLower();
        var index = _client.Index(IndexName);
        await index.DeleteOneDocumentAsync(documentId, cancellationToken: cancellationToken);
    }
}