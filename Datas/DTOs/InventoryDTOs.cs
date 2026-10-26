using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using ChatSystem.Models;

namespace ChatSystem.DTOs.Inventory;
public record ProductDetailsDTO
(
    string Name,
    string? Description,
    [Range(0, double.MaxValue, ErrorMessage = "Base price cannot be negative")]
    decimal Baseprice,
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    int Stock,
    ProductMode Mode,
    IFormFile? ProductIMG,
    string? FileName
);

public record UpdateProductDetailsDTO
(
    string ProductId,

    [Required]
    [StringLength(100)]
    string NewName,

    [StringLength(500)]
    string? NewDescription,

    decimal NewBasePrice,
    int Quantity,

    [EnumDataType(typeof(IsAddOrRemove))]
    IsAddOrRemove IsAddOrRemove
);
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IsAddOrRemove
{
    Add,
    Remove
}
public record UpdateProductStatusDTO
(
    string ProductId,
    bool NewStatus 
);

public record ProductSummaryDto(
    string Id,           
    string ProductName,
    decimal BasePrice,
    bool IsAvailable,
    ProductMode Mode
);
public record ProductDetailDto(
    string Id,
    string ProductName,
    string? ProductDescription,
    decimal BasePrice,
    int Stock,
    int ProductAvailable,
    int ReservedProduct,
    ProductMode Mode,
    bool IsAvailable,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);