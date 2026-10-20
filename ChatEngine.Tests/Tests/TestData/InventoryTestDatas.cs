using ChatSystem.DTOs.Inventory;
using ChatSystem.Models;
namespace ChatEngine.Tests.InventoryDatas;
public class InventoryTestDatas
{
    public static IEnumerable<object[]> CreateProductData()
    {
        yield return new object[]
        {
            new ProductDetailsDTO("TestProduct1", "TestProductDescription", 100m, 4, ProductMode.ForSaleOnly),
            200, 
            "testUser",
            "TestuserPassword",
            ""
        };
        yield return new object[]
        {
            new ProductDetailsDTO("Freebie", null, 0m, 0, ProductMode.ForSaleOnly),
            200, 
            "testUser",
            "TestuserPassword",
            ""
        };
        yield return new object[]
        {
            new ProductDetailsDTO("TestProduct3", "Description", -100m, 4, ProductMode.ForSaleOnly),
            400, // Bad Request due to validation
            "testUser",
            "TestuserPassword",
            ""
        };
        yield return new object[]
        {
            new ProductDetailsDTO("TestProduct4", "Description", 100m, -4, ProductMode.ForSaleOnly),
            400, // Bad Request due to validation
            "testUser",
            "TestuserPassword",
            ""
        };
        yield return new object[]
        {
            new ProductDetailsDTO(null!, "Description", 100m, 4, ProductMode.ForSaleOnly),
            400, 
            "testUser",
            "TestuserPassword",
            ""
        };
        yield return new object[]
        {
            new ProductDetailsDTO("TestProduct5", "Description", 100m, 4, ProductMode.ForSaleOnly),
            401, // Unauthorized
            "testUser",
            "TestuserPassword",
            "intentionally_invalid_jwt_string"
        };
    }
    
    public static IEnumerable<object[]> UpdateProductDatas()
    {
        var valid = new UpdateProductDetails_Test_DTO(
            new ProductDetailsDTO("Original product", "Original description", 100m, 10, ProductMode.ForSaleOnly),
            "Updated product", "Updated description", 125.50m, 2,
            "inventoryOwner", "InventoryPassword123!");

        static object[] Case(string name, UpdateProductDetails_Test_DTO data, int status)
            => new object[] { data with { CaseName = name }, status };

        // Successful updates must also persist the exact fields and stock delta.
        yield return Case("Add stock", valid, 200);
        yield return Case("Remove some stock", valid with { IsAddOrRemove = IsAddOrRemove.Remove }, 200);
        yield return Case("Remove all stock", valid with { Quantity = 10, IsAddOrRemove = IsAddOrRemove.Remove }, 200);
        yield return Case("Zero quantity Add updates details only", valid with { Quantity = 0 }, 200);
        yield return Case("Zero quantity Remove updates details only", valid with { Quantity = 0, IsAddOrRemove = IsAddOrRemove.Remove }, 200);
        yield return Case("Restock empty product", valid with { ProductDetails = valid.ProductDetails with { Stock = 0 } }, 200);
        yield return Case("Remove zero from empty product", valid with { ProductDetails = valid.ProductDetails with { Stock = 0 }, Quantity = 0, IsAddOrRemove = IsAddOrRemove.Remove }, 200);
        yield return Case("Free product", valid with { NewBasePrice = 0m }, 200);
        yield return Case("Small positive price", valid with { NewBasePrice = 0.01m }, 200);
        // There is currently no configured price ceiling or decimal scale.
        yield return Case("Maximum decimal price", valid with { NewBasePrice = decimal.MaxValue }, 200);
        yield return Case("Fractional price is preserved", valid with { NewBasePrice = 1.2345m }, 200);
        yield return Case("Null description clears description", valid with { NewDescription = null }, 200);
        yield return Case("Empty description", valid with { NewDescription = "" }, 200);
        yield return Case("Whitespace description is optional text", valid with { NewDescription = " \t " }, 200);
        yield return Case("One character name", valid with { NewName = "A" }, 200);
        yield return Case("Name at 100 character limit", valid with { NewName = new string('N', 100) }, 200);
        yield return Case("Description at 500 character limit", valid with { NewDescription = new string('D', 500) }, 200);
        yield return Case("Unicode and punctuation", valid with { NewName = "Soldering kit — 日本語", NewDescription = "Owner's kit\n5V & 3.3V <test>" }, 200);
        yield return Case("Trade only product", valid with { ProductDetails = valid.ProductDetails with { Mode = ProductMode.ForTradeOnly } }, 200);
        yield return Case("Sale and trade product", valid with { ProductDetails = valid.ProductDetails with { Mode = ProductMode.AcceptsBoth } }, 200);

        // Invalid numeric inputs: reject without changing any stored fields.
        yield return Case("Negative quantity Add", valid with { Quantity = -1 }, 400);
        yield return Case("Negative quantity Remove", valid with { Quantity = -1, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("Minimum integer quantity", valid with { Quantity = int.MinValue, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("Negative price", valid with { NewBasePrice = -0.01m }, 400);
        yield return Case("Minimum decimal price", valid with { NewBasePrice = decimal.MinValue }, 400);
        yield return Case("Remove one more than stock", valid with { Quantity = 11, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("Remove from empty product", valid with { ProductDetails = valid.ProductDetails with { Stock = 0 }, Quantity = 1, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("Remove maximum integer from small stock", valid with { Quantity = int.MaxValue, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("Reach maximum stock exactly", valid with { Quantity = int.MaxValue - 10 }, 200);
        yield return Case("Exceed maximum stock by one", valid with { Quantity = int.MaxValue - 9 }, 400);
        yield return Case("Add maximum quantity to nonempty stock", valid with { Quantity = int.MaxValue }, 400);

        // Stock includes reserved items; available stock excludes them.
        yield return Case("Add preserves reservations", valid with { ReservedStock = 4 }, 200);
        yield return Case("Remove exactly available stock", valid with { ReservedStock = 4, Quantity = 6, IsAddOrRemove = IsAddOrRemove.Remove }, 200);
        yield return Case("Cannot remove reserved stock", valid with { ReservedStock = 4, Quantity = 7, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("All stock reserved cannot remove one", valid with { ReservedStock = 10, Quantity = 1, IsAddOrRemove = IsAddOrRemove.Remove }, 400);
        yield return Case("All stock reserved can update details", valid with { ReservedStock = 10, Quantity = 0 }, 200);

        // Desired validation contract. Failures here reveal missing API validation;
        // do not change these to expect 200 or 500 just to make the suite pass.
        yield return Case("Null name", valid with { NewName = null! }, 400);
        yield return Case("Empty name", valid with { NewName = "" }, 400);
        yield return Case("Whitespace name", valid with { NewName = " \t\r\n " }, 400);
        yield return Case("Name exceeds 100 characters", valid with { NewName = new string('N', 101) }, 400);
        yield return Case("Description exceeds 500 characters", valid with { NewDescription = new string('D', 501) }, 400);
        yield return Case("Undefined positive operation", valid with { IsAddOrRemove = (IsAddOrRemove)2 }, 400);
        yield return Case("Undefined negative operation", valid with { IsAddOrRemove = (IsAddOrRemove)(-1) }, 400);

        yield return Case("Null product ID", valid with { OverrideProductId = true, ProductIdOverride = null }, 400);
        yield return Case("Empty product ID", valid with { OverrideProductId = true, ProductIdOverride = "" }, 400);
        yield return Case("Whitespace product ID", valid with { OverrideProductId = true, ProductIdOverride = "   " }, 400);
        yield return Case("Malformed product ID", valid with { OverrideProductId = true, ProductIdOverride = "!!!invalid!!!" }, 400);
        yield return Case("Valid hash for nonexistent product", valid with { UseMissingProductId = true }, 404);
        yield return Case("Missing bearer token", valid with { OmitAuthorization = true }, 401);
        yield return Case("Tampered bearer token", valid with { JwtInvalidator = "_invalid" }, 401);
        yield return Case("Another user cannot update owner's product", valid with { UseAnotherUser = true }, 403);
    }
}
