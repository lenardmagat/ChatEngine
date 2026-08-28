using ChatSystem.core;
using ChatSystem.DTOs.Documentation;
using ChatSystem.DTOs.Search;
using ChatSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ChatSystem.EventHandler.Search;
public class ProductSearchStrategy : ISearchStrategy
{
    private readonly IDynamicSearchService _searchService;
    private readonly IHasher _hasher;
    private readonly ILogger<ProductSearchStrategy> _logger;
    public ProductSearchStrategy(IDynamicSearchService searchService, IHasher hasher, ILogger<ProductSearchStrategy> logger)
    {
        _searchService = searchService;
        _hasher = hasher;
        _logger = logger;
    }
    public SearchTarget Target => SearchTarget.Products;
    public async Task<PagedResult<object>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        string? typeFilter = request.Filters?.GetValueOrDefault("Mode");
        string meiliFilter = string.IsNullOrWhiteSpace(typeFilter)
            ? "isActive = 'True' AND isAvailable = 'True'"
            : $"productStatus = '{typeFilter}' AND isActive = 'True' AND isAvailable = 'True'";
        _logger.LogInformation("Built filter: {Filter}", meiliFilter);
        PagedResult<ProductDocumentation> rawpagedProduct;
        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            rawpagedProduct = await _searchService.SearchWithFilterAsync<ProductDocumentation>(
                request.Term,
                meiliFilter,
                request.Page,
                request.PageSize,
                cancellationToken
            );
        }
        else
        {
            rawpagedProduct = await _searchService.SearchAsync<ProductDocumentation>(
                request.Term,
                request.Page,
                request.PageSize,
                cancellationToken
            );
        }
        if(rawpagedProduct is null)
        {
            return new()
            {
                Items = null,
                TotalCount = 0,
                Page = 0,
                PageSize = 0
            };
        }
        PagedResult<ProductSearchDTOResponse> pagedResult = rawpagedProduct
            .Select(d => new ProductSearchDTOResponse(
                _hasher.CreateHashids(int.Parse(d.id), HashContext.Product),
                d.ProductName,
                decimal.Parse(d.BasePrice),
                int.Parse(d.QuantityAvailable),
                d.ProductStatus
            ));
        return pagedResult.CastToObjectMapper();
    }
}