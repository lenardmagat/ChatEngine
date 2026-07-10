using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Features.Chats;
public class InitializeChatCommandHandler : IRequestHandler<InitializeChatCommand, Result<ChatData?>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public InitializeChatCommandHandler(DbManager db, IHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<ChatData?>> Handle(InitializeChatCommand command, CancellationToken cancellation)
    {
        ChatData? ChatDatas;
        if(string.IsNullOrEmpty(command.ChatId) && !string.IsNullOrEmpty(command.RecieverId))
        {
            var chatMessages = await _db.Chatrooms
                                .Where(c => 
                                c.Participants.Any(p => p.UserId == command.UserId) &&
                                c.Participants.Any(p => p.UserId == _hasher.DecodeHashids(command.RecieverId).Value)
                                )
                                .Select(r => new
                                {
                                    RoomId = r.Id,
                                    ReceiverId = r.Participants
                                        .Where(p => p.UserId != command.UserId)
                                        .Select(u => u.UserId)
                                        .FirstOrDefault(),
                                    LastMessageTimeStampt = r.Messages
                                        .Max(m => (DateTime?)m.TimeStamp ?? DateTime.MinValue),
                                    RecentMessages = r.Messages
                                        .OrderByDescending(m => m.Id)
                                        .Take(15)
                                        .Select(m => new
                                        {
                                            chatId = m.Id,
                                            senderName = m.Sender.Username,
                                            senderId = m.SenderId,
                                            chatMessage = m.MessageText,
                                            timeStampt = m.TimeStamp 
                                        }
                                        ).ToList()
                                }
                                ).FirstOrDefaultAsync();
        if(chatMessages is null)
            {
                return Result<ChatData?>.Success(new ChatData(true, null, null ,null ,null));
            }
            List<MessageData>? messageDatas = chatMessages.RecentMessages.Select(m => new 
                MessageData(
                    ChatId : _hasher.CreateHashids(m.chatId),
                    ChatMessage : m.chatMessage,
                    TimeStampt: m.timeStampt,
                    SenderName : m.senderName,
                    SenderId : _hasher.CreateHashids(m.senderId)
                )
            ).ToList();

        ChatDatas = new ChatData(
            IsNew : false,
            RoomId : _hasher.CreateHashids(chatMessages.RoomId),
            LastTimeStampt : chatMessages.LastMessageTimeStampt,
            RecieverId : _hasher.CreateHashids(chatMessages.ReceiverId),
            MessageDatas : messageDatas
        );
        return Result<ChatData?>.Success(ChatDatas);
        }
        else if(string.IsNullOrEmpty(command.RecieverId) && !string.IsNullOrEmpty(command.ChatId))
        {
            var chatMessages = await _db.Chatrooms
                                .Where(m => m.Id == _hasher.DecodeHashids(command.ChatId).Value)
                                .Select(r => new
                                {
                                    receiverId = _hasher
                                                    .CreateHashids(
                                                        r.Participants
                                                        .Where(p => p.UserId != command.UserId)
                                                        .Select(u => u.UserId)
                                                        .FirstOrDefault()),
                                    LastMessageTimeStampt = r.Messages
                                                    .Max(m => (DateTime?)m.TimeStamp ?? DateTime.MinValue),
                                    RecentMessages = r.Messages
                                        .OrderByDescending(m => m.Id)
                                        .Take(15)
                                        .Select(m => new
                                        {
                                            chatId = m.Id,
                                            senderName = m.Sender.Username,
                                            senderId = m.SenderId,
                                            chatMessage = m.MessageText,
                                            timeStampt = m.TimeStamp 
                                        }
                                        ).ToList()
                                }
                                ).FirstOrDefaultAsync();
            if(chatMessages is null)
            {
                return Result<ChatData?>.Failure("Invalid Credentials", StatusCodes.Status401Unauthorized);
            }
            List<MessageData>? messageDatas = chatMessages.RecentMessages.Select(m => new 
                MessageData(
                    ChatId : _hasher.CreateHashids(m.chatId),
                    ChatMessage : m.chatMessage,
                    TimeStampt: m.timeStampt,
                    SenderName : m.senderName,
                    SenderId : _hasher.CreateHashids(m.senderId)
                )
            ).ToList();
            ChatDatas = new ChatData(
            IsNew : false,
            RoomId : command.ChatId,
            LastTimeStampt : chatMessages.LastMessageTimeStampt,
            RecieverId : chatMessages.receiverId,
            MessageDatas : messageDatas
        );
        return Result<ChatData?>.Success(ChatDatas);
        }
        return Result<ChatData?>.Failure("", StatusCodes.Status400BadRequest);
    }
}