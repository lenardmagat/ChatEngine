namespace ChatSystem.DTOs;
public class OfferResponse
{
    public int MessageId { get; set; }
    public int RoomId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string MessageType => $"Offer{Status}";
    public DateTime TimeStamp { get; set; }

    // --- Discriminator / Identifier ---
    public string OfferCategory { get; set; } = null!; // "Trade" or "Sale"
    public int OfferId { get; set; }
    public int? ParentOfferId { get; set; }
    public int ProposedByUserId { get; set; }
    public string Status { get; set; } = null!; // "Proposed", "Countered", "Accepted", etc.

    // --- Trade Specific Payload (Null if Sale) ---
    public TradePayloadDto? TradeDetails { get; set; }

    // --- Sale Specific Payload (Null if Trade) ---
    public SalePayloadDto? SaleDetails { get; set; }
}

public class TradePayloadDto
{
    public string ItemOffered { get; set; } = null!;
    public string ItemRequested { get; set; } = null!;
}
public class SalePayloadDto
{
    public int ListingId { get; set; }
    public string ItemName { get; set; } = null!;
    public int QuantityRequested { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalPrice => QuantityRequested * PricePerUnit;
}
public record SaleOfferResponseDTO(
    string OfferId,
    string ItemId,
    string ItemName,
    int QuantityRequested,
    decimal PricePerUnit,
    decimal TotalPrice,
    string Status,
    string ProposedByUserName,
    DateTime CreatedAt
);