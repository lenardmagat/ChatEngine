namespace ChatSystem.DTOs;
public record SendMessage(
    string? ChatId,
    string Message,
    string? RecieverId
);
public record MessageResponseDTO(
    string NewMessageId,
    string RoomId,
    string SenderName,
    string NewMessage,
    string TimeStampt
     
);

public record ChatData(
    string ChatId,
    string ChatMessage,
    DateTime TimeStampt,
    string SenderName,
    string SenderId,
    string RecieverId
);
public record LoadConversationResponse(
    string RoomId,
    DateTime LastTimeStampt,
    string RecieverId
);