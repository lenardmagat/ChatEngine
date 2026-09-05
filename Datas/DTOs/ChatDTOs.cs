using System.Text.Json.Serialization;
using ChatSystem.Models;

namespace ChatSystem.DTOs;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OfferTye
{
    Sale,
    Trade
}
public enum OfferStatus
{
    Proposed,   
    Countered, 
    Accepted, 
    Declined, 
    Cancelled,  
    Expired,   
    Completed  
}
public record OfferPayload(
    OfferTye Tye,
    OfferStatus Status,
    int offerId
);
public record SendMessage(
    string? RoomId,
    string Message,
    string? RecieverId,
    MessageType Type,
    OfferPayload? OfferPayload
);
public record MessageResponseDTO(
    string RoomId,
    string ReceipientId,
    MessageData MessageData
);
    
public record MessageData(
    string ChatId,
    string ChatMessage,
    DateTime TimeStampt,
    string SenderName,
    string SenderId,
    SaleOfferResponseDTO? SaleOfferDetails,
    MessageType Type = MessageType.Text,
    OfferTye? OfferCategory = null
);
public record ChatData(
    bool IsNew,
    string? RoomId,
    DateTime? LastTimeStampt,
    string? RecieverId,
    List<MessageData>? MessageDatas
);
public record LoadConversationResponse(
    string RoomId,
    DateTime LastTimeStampt,
    string RecieverId,
    string ReceiverName
);
public record RoomDataDTO(
    int RoomId,
    int ReceiverId,
    string ReceiverUsername
);
public class MessageSummaryDto
{
    public int ChatId { get; set; }
    public string SenderName { get; set; } = null!;
    public int SenderId { get; set; }
    public string ChatMessage { get; set; } = null!;
    public DateTime TimeStampt { get; set; }
    public MessageType Type { get; set; }
    public OfferTye OfferCategory { get; set; }
    public SaleOffer? saleOffer {get; set;}
    public TradeOffer? tradeOffer {get; set;}
}

public class ParticipantSummaryDto
{
    public int ParticipantId { get; set; }
    public bool IsCurrentUser { get; set; }
}   