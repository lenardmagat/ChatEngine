using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ChatSystem.Models;
public enum TradeOfferStatus
{
    Proposed,
    Countered,
    Accepted,
    Declined,
    Cancelled,
    Completed
}

public class TradeOffer
{
    [Key]
    public int Id { get; set; }

    public int RoomId { get; set; }
    [ForeignKey("RoomId")]
    public ChatRoom Room { get; set; } = null!;

    public int ProposedByUserId { get; set; }
    [ForeignKey("ProposedByUserId")]
    public User ProposedBy { get; set; } = null!;
    public int? ParentOfferId { get; set; }
    [ForeignKey("ParentOfferId")]
    public TradeOffer? ParentOffer { get; set; }
    public string ItemOffered { get; set; } = null!;
    public string ItemRequested { get; set; } = null!;

    public TradeOfferStatus Status { get; set; } = TradeOfferStatus.Proposed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}

