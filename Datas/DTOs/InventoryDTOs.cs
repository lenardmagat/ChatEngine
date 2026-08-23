using System.ComponentModel.DataAnnotations;
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
    ProductMode Mode
);

public record UpdateProductDetailsDTO
(
    string ProductId,
    string NewName,
    string? NewDescription,
    string UserPassword,
    decimal NewBasePrice,
    int Quantity,
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
    ProductMode NewMode, 
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    int Quantity,
    string UserPassword,
    IsAddOrRemove AddOrRemove
);