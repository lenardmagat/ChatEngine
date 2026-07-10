using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.DataBase;
namespace ChatSystem.Features.Chats;
public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<MessageResponseDTO>>
{
    private readonly IMediator _mediator;
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    public SendMessageCommandHandler(IMediator mediator, DbManager db, IHasher hasher)
    {
        _mediator = mediator;
        _db = db;
        _hasher = hasher;
    }
    public async Task<Result<MessageResponseDTO>> Handle(SendMessageCommand request, CancellationToken cancellation)
    {
        Result<int> ChatId = _hasher.DecodeHashids(request.MessageData.ChatId);
        User? user = await _db.Chatrooms
                            .Where(r => r.Id == ChatId.Value && r.Participants
                            .Any(p => p.UserId == request.UserId))
                            .SelectMany(a => a.Participants)
                            .Select(p => p.User)
                            .Where(u => u.UserId == request.UserId)
                            .FirstOrDefaultAsync();
        if(user is null) 
            {
                return Result<MessageResponseDTO>.Failure("Invalid Credential", StatusCodes.Status403Forbidden);
            }
        ChatMessage NewMessageData = new ChatMessage
        {
            RoomId = ChatId.Value,
            Sender = user!,
            MessageText = request.MessageData.Message
        };
        await _db.Messages.AddAsync(NewMessageData, cancellation);
        MessageResponseDTO responseDTO = new MessageResponseDTO
        (
            NewMessageId: _hasher.CreateHashids(NewMessageData.Id),
            RoomId : request.MessageData.ChatId,
            SenderName : user!.Username,
            NewMessage : NewMessageData.MessageText,
            TimeStampt : NewMessageData.TimeStamp.ToString()
        );
        await _db.SaveChangesAsync();
        return Result<MessageResponseDTO>.Success(responseDTO);
    }
}