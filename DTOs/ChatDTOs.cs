namespace ChatSystem.DTOs;
public record SendMessage(
    string ChatId,
    string Message
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
    string SenderId
);
public record LoadConversationResponse(
    string RoomId,
    DateTime LastTimeStampt,
    List<ChatData> Chats
);