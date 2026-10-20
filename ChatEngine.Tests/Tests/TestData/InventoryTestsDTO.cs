using ChatSystem.DTOs.Inventory;

namespace ChatEngine.Tests.InventoryDatas;
public record UpdateProductDetails_Test_DTO(
    ProductDetailsDTO ProductDetails,
    string NewName, 
    string? NewDescription,
    decimal NewBasePrice,
    int Quantity,
    string TestUsername,
    string TestPassword,
    string? JwtInvalidator = null,
    IsAddOrRemove IsAddOrRemove = IsAddOrRemove.Add
)
{
    public string CaseName { get; init; } = "Update product";
    public int ReservedStock { get; init; }
    public bool OverrideProductId { get; init; }
    public string? ProductIdOverride { get; init; }
    public bool UseMissingProductId { get; init; }
    public bool UseAnotherUser { get; init; }
    public bool OmitAuthorization { get; init; }

    public override string ToString() => CaseName;
}
