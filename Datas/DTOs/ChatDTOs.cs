using ChatSystem.Models;

namespace ChatSystem.DTOs;
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
    int ItemId
);
public record SendMessage(
    string? RoomId,
    string Message,
    string? RecieverId,
    MessageType Type,
    OfferPayload? OfferPayload
);
public record MessageResponseDTO(
    string NewMessageId,
    string RoomId,
    string SenderName,
    string NewMessage,
    string TimeStampt,
    string ReceipientId
     
);
    
public record MessageData(
    string ChatId,
    string ChatMessage,
    DateTime TimeStampt,
    string SenderName,
    string SenderId
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
}

public class ParticipantSummaryDto
{
    public int ParticipantId { get; set; }
    public bool IsCurrentUser { get; set; }
}   