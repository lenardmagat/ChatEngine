using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChatSystem.Models;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductMode
{
    ForSaleOnly,
    ForTradeOnly,
    AcceptsBoth,
    DeclineBoth
}
public class Product
{
    [Key]
    public int Id {get; set;}
    public int OwnerUserId{get; set;}
    [ForeignKey("OwnerUserId")]
    public User Owner {get; set;} = null!;

    [Required]
    [MaxLength(100)]
    public string ProductName {get; set;} = null!;
    [MaxLength(500)]
    public string? ProductDescription {get; set;}

    [Range(0, double.MaxValue, ErrorMessage = "Base price cannot be negative")]
    public decimal BasePrice {get; set;}
    [Range(0, int.MinValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock {get; set;}
    [Range(0, int.MaxValue)]
    public int ProductAvailable {get; set;}
    [Range(0, int.MaxValue)]
    public int ReservedProdcut {get; set;}
    public ProductMode Mode{get; set;}
    public bool IsAvailable => ProductAvailable > 0 && Mode != ProductMode.DeclineBoth;
    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime? UpdatedA {get; set;}
}