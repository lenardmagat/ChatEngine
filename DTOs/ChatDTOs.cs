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
    string RecieverId
);