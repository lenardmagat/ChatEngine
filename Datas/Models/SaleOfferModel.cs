using System.ComponentModel.DataAnnotations.Schema;
using System.Security;
using ChatSystem.ErrorHandling;
using Microsoft.VisualBasic;

namespace ChatSystem.Models;
public enum SaleOfferStatus
{
    Proposed,   // Buyer sent an offer; stock is reserved
    Countered,  // Seller or Buyer responded with a different price/quantity
    Accepted,   // Offer agreed upon; moving to checkout/payment
    Declined,   // Receiver rejected the offer; stock reservation is released
    Cancelled,  // Sender withdrew their offer before response; stock released
    Expired,    // No action was taken within the timeout period; stock released automatically
    Completed   // Payment processed and item delivered successfully
}
public class SaleOffer
{
    public int Id {get; set;}
    public int RoomId {get; set;}
    [ForeignKey("RoomId")]
    public ChatRoom Room {get; set;} = null!;
    public int ProposedByUserId {get; set;} 
    [ForeignKey("ProposedByUserId")]
    public User UserProposed {get; set;} = null!;
    public int? ParentId {get; set;}
    [ForeignKey("ParentId")]
    public SaleOffer? ParentSaleOffer;
    public int ItemId {get; set;}
    [ForeignKey("ItemId")]
    public Product ItemDetails {get; set;} = null!;
    public int QuantityRequested {get; set;}
    public decimal PricePerUnit {get; set;}
    public SaleOfferStatus Status {get; set;}
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    private static readonly Dictionary<SaleOfferStatus, SaleOfferStatus[]> _AllowedToTransition = new()
    {
        [SaleOfferStatus.Proposed] = 
            [
                SaleOfferStatus.Accepted, 
                SaleOfferStatus.Countered, 
                SaleOfferStatus.Declined, 
                SaleOfferStatus.Cancelled, 
                SaleOfferStatus.Expired
            ],
        [SaleOfferStatus.Accepted] = 
            [
                SaleOfferStatus.Completed, 
                SaleOfferStatus.Cancelled
            ],
        [SaleOfferStatus.Countered] = [],
        [SaleOfferStatus.Declined]  = [],
        [SaleOfferStatus.Cancelled] = [],
        [SaleOfferStatus.Expired]   = [],
        [SaleOfferStatus.Completed] = []
    };

    public Result TransitionTo(SaleOfferStatus next)
    {
        if(!_AllowedToTransition.TryGetValue(Status, out var allowed) || !allowed.Contains(next))
        {
            return Result.Failure($"Cannot move offer from {Status} to {next}.", 409);
        }
        Status = next;
        return Result.Success();
    }
}