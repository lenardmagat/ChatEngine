using ChatEngine.InfrastructureDeletion;
using ChatEngine.TestFactory;
using ChatEngine.Tests.Services;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using ChatEngine.Tests.AccountDatas;
using Microsoft.VisualBasic;
using System.Reflection;
using ChatSystem.DTOs.Inventory;
using ChatEngine.Tests.BaseTests;
using ChatEngine.Tests.InventoryDatas;
namespace ChatEngine.Tests.InventoryTests;
[Collection("ChatEngineTest")]
public class InventoryTest : CoreIntegrationTestBase
{
    protected readonly AuthenticatedUserTestServices _authService;
    protected readonly InventoryTestServices _inventoryService;
    public InventoryTest(ChatEngineTestFactory factory) : base(factory)
    {
        _authService = new AuthenticatedUserTestServices(factory);
        _inventoryService =  new InventoryTestServices(factory);
    }
    
    [Theory]
    [MemberData(nameof(InventoryTestDatas.CreateProductData), MemberType = typeof(InventoryTestDatas))]
    public async Task CreateProduct_InventoryRouter_Test(ProductDetailsDTO productDetails, int expectedStatusCode, string testUsername, string testUserPassword, string? jwtInvalidator = null)
    {
        var TestUserToken = await _authService.RegisterAndLogin(testUsername, testUserPassword);
        var createProductResponse = await _inventoryService.CreateProductTest(productDetails, TestUserToken + jwtInvalidator);
        var body = createProductResponse.Content.ReadAsStringAsync();
        Assert.True(expectedStatusCode == (int)createProductResponse.StatusCode, $"Expected: {expectedStatusCode}, but got {(int)createProductResponse.StatusCode}. Body: {body}");
    }

    [Theory]
    [MemberData(nameof(InventoryTestDatas.UpdateProductDatas), MemberType = typeof(InventoryTestDatas))]
    public async Task Updateproduct_InventoryRouter_Test(UpdateProductDetails_Test_DTO details_Test_DTO, int expectedCodeStatus)
    {
        using HttpResponseMessage httpResponse = await _inventoryService.UpdateProductDetailsTest(details_Test_DTO);
        string body = await httpResponse.Content.ReadAsStringAsync();
        Assert.True((int)httpResponse.StatusCode == expectedCodeStatus,
            $"{details_Test_DTO.CaseName}: expected {expectedCodeStatus}, got {(int)httpResponse.StatusCode}. Body: {body}");

        // Each theory row starts with a reset database and creates one product.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbManager>();
        var product = await db.Products.AsNoTracking().SingleAsync();
        bool succeeded = expectedCodeStatus == 200;
        int delta = succeeded
            ? (details_Test_DTO.IsAddOrRemove == IsAddOrRemove.Add ? details_Test_DTO.Quantity : -details_Test_DTO.Quantity)
            : 0;
        Assert.Equal(details_Test_DTO.ProductDetails.Stock + delta, product.Stock);
        Assert.Equal(details_Test_DTO.ProductDetails.Stock - details_Test_DTO.ReservedStock + delta, product.ProductAvailable);
        Assert.Equal(details_Test_DTO.ReservedStock, product.ReservedProdcut);
        Assert.Equal(succeeded ? details_Test_DTO.NewName : details_Test_DTO.ProductDetails.Name, product.ProductName);
        Assert.Equal(succeeded ? details_Test_DTO.NewDescription : details_Test_DTO.ProductDetails.Description, product.ProductDescription);
        Assert.Equal(succeeded ? details_Test_DTO.NewBasePrice : details_Test_DTO.ProductDetails.Baseprice, product.BasePrice);
        Assert.Equal(details_Test_DTO.ProductDetails.Mode, product.Mode);
        if (succeeded)
            Assert.NotNull(product.UpdatedA);
        else
            Assert.Null(product.UpdatedA);
    }
}
