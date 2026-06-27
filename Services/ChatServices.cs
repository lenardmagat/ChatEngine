using ChatSystem.DataBase;
using ChatSystem.core;
using ChatSystem.ErrorHandling;
using ChatSystem.DTOs;
using Microsoft.EntityFrameworkCore;
using ChatSystem.Models;
namespace ChatSystem.Services;
public interface IChatServices
{
    public Task<Result<MessageResponseDTO>> DirectMessage(int userId, SendMessage messageData);
}
public class ChatServices : IChatServices
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public ChatServices(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Result<MessageResponseDTO>> DirectMessage(int userId, SendMessage messageData)
    {
        if (_db.Users.Where(u => u.UserId == userId ).FirstOrDefaultAsync() == null) return Result<MessageResponseDTO>.Failure("Invalid Credentials", 404);
        if(string.IsNullOrWhiteSpace(messageData.Message))
            return Result<MessageResponseDTO>.Failure("Message cannot be empty.", 400);
        Result<ChatRoom> chatRoom = await GetorCreateChatRoom(messageData.RoomId, messageData.RecipientId, userId);
        if(!chatRoom.IsSuccess) return Result<MessageResponseDTO>.Failure(chatRoom.Error!, chatRoom.StatusCode);
        ChatMessage NewMessage = new ChatMessage
        {
            RoomId = messageData.RoomId,
            SenderId = userId,
            MessageText = messageData.Message
        };
        await _db.AddAsync(NewMessage);
        await _db.SaveChangesAsync();
        MessageResponseDTO responseDTO = new MessageResponseDTO
        (
            NewMessageId: await Task.Run(() => _hasher.CreateHashids(NewMessage.Id)),
            RoomId: await Task.Run(() => _hasher.CreateHashids(messageData.RoomId)),
            SenderId: await Task.Run(() => _hasher.CreateHashids(userId)),
            NewMessage: messageData.Message,
            TimeStampt: NewMessage.TimeStamp.ToString()
        );

        return Result<MessageResponseDTO>.Success(responseDTO);
        
    }

    private async Task<Result<ChatRoom>> GetorCreateChatRoom(int roomId, string recipientId, int userId)
    {
        Result<int> RecipientId = await Task.Run(() => _hasher.DecodeHashids(recipientId));
        if(RecipientId.IsSuccess == false) return Result<ChatRoom>.Failure("Invalid Credentials", 400);
        ChatRoom? chatRoom = await _db.Chatrooms.Where(c => c.Id == roomId && c.Participants.Any(p => p.UserId == userId || p.UserId == RecipientId.Value)).FirstOrDefaultAsync();
        if(chatRoom is not null)
        {
            return Result<ChatRoom>.Success(chatRoom);
        }
        ChatRoom newChatRoom = new ChatRoom
        {
            IsGroupChat = false,

        };
        var Participants = new List<RoomParticipant>{
            new RoomParticipant
            {
                RoomId = newChatRoom.Id,
                UserId = userId
            },
            new RoomParticipant
            {
                RoomId = newChatRoom.Id,
                UserId = RecipientId.Value
            }
        };
        await _db.AddAsync(newChatRoom);
        await _db.AddRangeAsync(Participants);
        await _db.SaveChangesAsync();
        return Result<ChatRoom>.Success(newChatRoom);
    }
}