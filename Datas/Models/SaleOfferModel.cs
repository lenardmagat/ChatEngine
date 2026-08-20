using System.ComponentModel.DataAnnotations.Schema;

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
    public int QuantityRequested {get; set;}
    public decimal PricePerUnit {get; set;}
    public SaleOfferStatus Status {get; set;}
}