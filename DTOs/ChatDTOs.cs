namespace ChatSystem.DTOs;
public record SendMessage(
    string RecipientId,
    int RoomId,
    string Message
);
public record MessageResponseDTO(
    string NewMessageId,
    string RoomId,
    string SenderId,
    string NewMessage,
    string TimeStampt
     
);

// newMessage.Id,
//             chat.Id,
//             senderId,
//             newMessage.MessageText,
//             newMessage.TimeStamp