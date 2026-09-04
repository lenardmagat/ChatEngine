using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSystem.Models;

public class SaleOfferEvent
{
    public long Id { get; set; }
    public int SaleOfferId { get; set; }
    [ForeignKey(nameof(SaleOfferId))]
    public SaleOffer SaleOffer { get; set; } = null!;
    public int Version { get; set; }
    public SaleOfferStatus? FromStatus { get; set; }
    public SaleOfferStatus ToStatus { get; set; }
    public decimal PricePerUnit { get; set; }
    public int QuantityRequested { get; set; }
    public int ActorUserId { get; set; }
    [ForeignKey(nameof(ActorUserId))]
    public User Actor { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}