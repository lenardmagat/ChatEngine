using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatSystem.DTOs.Inventory;
using ChatEngine.TestFactory;
using ChatSystem.DataBase;
using Microsoft.Extensions.DependencyInjection;
using ChatEngine.Tests.InventoryDatas;
using ChatSystem.core;
using Microsoft.EntityFrameworkCore;
namespace ChatEngine.Tests.Services;
public class InventoryTestServices
{
    protected readonly ChatEngineTestFactory _factory;
    protected readonly HttpClient _client;
    protected readonly AuthenticatedUserTestServices _authServices;
    protected readonly DbManager _db;
    protected readonly IHasher _hasher;
    public InventoryTestServices(ChatEngineTestFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _authServices = new AuthenticatedUserTestServices(_factory);
        _db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<DbManager>();
        _hasher = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IHasher>();
        
    }

    public async Task<HttpResponseMessage> CreateProductTest(ProductDetailsDTO productDetails, string jwt)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return await _client.PostAsJsonAsync("/Inventory/CreateProduct", productDetails);
    }

    public async Task<HttpResponseMessage> UpdateProductDetailsTest(UpdateProductDetails_Test_DTO details_Test_DTO)
    {
        string testUserToken = await _authServices.RegisterAndLogin(details_Test_DTO.TestUsername, details_Test_DTO.TestPassword);
        Assert.NotEmpty(testUserToken);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", testUserToken);

        using HttpResponseMessage createProductResponse = await _client.PostAsJsonAsync("/Inventory/CreateProduct", details_Test_DTO.ProductDetails);
        createProductResponse.EnsureSuccessStatusCode();

        int productId = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductName == details_Test_DTO.ProductDetails.Name)
            .Select(d => d.Id)
            .SingleAsync();

        Assert.InRange(details_Test_DTO.ReservedStock, 0, details_Test_DTO.ProductDetails.Stock);
        if (details_Test_DTO.ReservedStock > 0)
        {
            await _db.Products.Where(p => p.Id == productId).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.ReservedProdcut, details_Test_DTO.ReservedStock)
                .SetProperty(p => p.ProductAvailable, details_Test_DTO.ProductDetails.Stock - details_Test_DTO.ReservedStock));
        }

        string? encodedProductId = _hasher.CreateHashids(productId, HashContext.Product);
        if (details_Test_DTO.UseMissingProductId)
        {
            Assert.False(await _db.Products.AnyAsync(p => p.Id == int.MaxValue));
            encodedProductId = _hasher.CreateHashids(int.MaxValue, HashContext.Product);
        }
        if (details_Test_DTO.OverrideProductId)
            encodedProductId = details_Test_DTO.ProductIdOverride;

        // Use the actual request DTO so the JSON field is ProductId, not ItemId.
        var updatePayload = new UpdateProductDetailsDTO(
            encodedProductId!,
            details_Test_DTO.NewName,
            details_Test_DTO.NewDescription,
            details_Test_DTO.NewBasePrice,
            details_Test_DTO.Quantity,
            details_Test_DTO.IsAddOrRemove);

        if (details_Test_DTO.UseAnotherUser)
            testUserToken = await _authServices.RegisterAndLogin("inventoryOther", details_Test_DTO.TestPassword);

        _client.DefaultRequestHeaders.Authorization = details_Test_DTO.OmitAuthorization
            ? null
            : new AuthenticationHeaderValue("Bearer", testUserToken + details_Test_DTO.JwtInvalidator);
        return await _client.PatchAsJsonAsync("/Inventory/UpdateProductDetails", updatePayload);
    }
}
