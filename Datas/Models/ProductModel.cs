using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSystem.Models;
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

    [Range(0, double.MinValue, ErrorMessage = "Base price cannot be negative")]
    public decimal BasePrice {get; set;}
    [Range(0, int.MinValue, ErrorMessage = "Quantity cannot be negative")]
    public int ProductQuantity {get; set;}
    [Range(0, int.MinValue)]
    public int ReservedProdcut {get; set;}
    public bool IsForSale {get; set;}
    public bool IsForTrade {get; set;}
    public bool IsAvailable => ProductQuantity > 0 && IsActive;
    public bool IsActive {get; set;}
    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime? UpdatedA {get; set;}
}