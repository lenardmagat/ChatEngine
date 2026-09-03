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
        string meiliFilter = (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<ChatSystem.Models.ProductMode>(typeFilter, true, out var validMode))
            ? $"productStatus = '{validMode}' AND isActive = 'True' AND isAvailable = 'True'"
            : "isActive = 'True' AND isAvailable = 'True'";
        _logger.LogInformation("Built filter: {Filter}", meiliFilter);
        PagedResult<ProductDocumentation> rawpagedProduct;
        rawpagedProduct = await _searchService.SearchWithFilterAsync<ProductDocumentation>(
            request.Term,
            meiliFilter,
            request.Page,
            request.PageSize,
            cancellationToken
        );
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