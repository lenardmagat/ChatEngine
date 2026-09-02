using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChatSystem.ErrorHandling;
namespace ChatSystem.Models;
public enum TradeOfferStatus
{
    Proposed,
    Countered,
    Accepted,
    Declined,
    Cancelled,
    Expired,
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
    public int ItemRequestedId { get; set; }
    [ForeignKey("ItemRequestedId")]
    public Product Product {get; set;} = null!;

    public TradeOfferStatus Status { get; set; } = TradeOfferStatus.Proposed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    private static readonly Dictionary<TradeOfferStatus, TradeOfferStatus[]> _allowedTransitions = new()
    {
        [TradeOfferStatus.Proposed]   = [
            TradeOfferStatus.Accepted, 
            TradeOfferStatus.Declined, 
            TradeOfferStatus.Cancelled, 
            TradeOfferStatus.Countered, 
            TradeOfferStatus.Expired
            ],
        [TradeOfferStatus.Accepted] = [
            TradeOfferStatus.Completed, 
            TradeOfferStatus.Cancelled
            ],
        [TradeOfferStatus.Completed] = [],
        [TradeOfferStatus.Countered] = [],
        [TradeOfferStatus.Cancelled] = [],
        [TradeOfferStatus.Declined] = [],
        [TradeOfferStatus.Expired] = []
    };
    public bool TransitionTo(TradeOfferStatus next)
    {
        if (!_allowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(next)) return false;
        return true;
    }
}

