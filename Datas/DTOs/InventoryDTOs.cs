using System.ComponentModel.DataAnnotations;
using ChatSystem.Models;

namespace ChatSystem.DTOs.Inventory;
public record ProductDetails
(
    string Name,
    string? Description,
    [Range(0, double.MaxValue, ErrorMessage = "Base price cannot be negative")]
    decimal Baseprice,
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    int Stock,
    ProductMode Mode
);

public record UpdateProductDetails
(
    string ProductId,
    string NewName,
    string? NewDescription,
    string UserPassword
);
public record UpdateProductStatus
(
    string ProductId,
    ProductMode NewMode, 
    [Range(0, int.MinValue, ErrorMessage = "Stock cannot be negative")]
    int NewStock,
    string UserPassword
);