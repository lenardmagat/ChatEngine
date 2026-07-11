using ChatSystem.SystemEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.DataBase;
using ChatSystem.core;
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
        int? targetRoomid = !string.IsNullOrEmpty(request.MessageData.RoomId)? _hasher.DecodeHashids(request.MessageData.RoomId, HashContext.Room).Value : null;
        int? targetReceiverId = !string.IsNullOrEmpty(request.MessageData.RecieverId) ? _hasher.DecodeHashids(request.MessageData.RecieverId, HashContext.User).Value : null;
        if (!targetRoomid.HasValue && !targetReceiverId.HasValue)
        {
            return Result<MessageResponseDTO>.Failure("Invalid request parameters.", StatusCodes.Status400BadRequest);
        }
        var roomData = await _db.Chatrooms
        .AsNoTracking()
        .Where(r => targetRoomid.HasValue 
            ? r.Id == targetRoomid.Value 
            : r.Participants.Any(p => p.UserId == request.UserId) && r.Participants.Any(p => p.UserId == targetReceiverId!.Value))
        .Select(r => new
        {
            RoomId = r.Id,
            IsSenderParticipant = r.Participants.Any(p => p.UserId == request.UserId),
            RecipientUserId = r.Participants
                .Where(p => p.UserId != request.UserId)
                .Select(p => p.UserId)
                .FirstOrDefault(),
            SenderName = r.Participants
                .Where(p => p.UserId == request.UserId)
                .Select(p => p.User.Username)
                .FirstOrDefault()
            
        })
        .FirstOrDefaultAsync(cancellation);
        int finalRoomId;
        int finalRecipientId;
        string Username;
        if (roomData != null)
        {
            if (!roomData.IsSenderParticipant)
            {
                return Result<MessageResponseDTO>.Failure("You do not have permission to post here.", StatusCodes.Status403Forbidden);
            }

            finalRoomId = roomData.RoomId;
            finalRecipientId = roomData.RecipientUserId;
            Username = roomData.SenderName!;
        }
        else
        {
            if (targetRoomid.HasValue)
            {
            return Result<MessageResponseDTO>.Failure("Chat session not found.", StatusCodes.Status404NotFound);
            }
            var verifiedUsers = await _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == request.UserId || u.UserId == targetReceiverId!.Value)
                .Select(ud => new {Id = ud.UserId, name = ud.Username})
                .ToListAsync();
            var recipientAccount = verifiedUsers.FirstOrDefault(u => u.Id == targetReceiverId!.Value);
            var senderAccount = verifiedUsers.FirstOrDefault(u => u.Id == request.UserId);
            if(recipientAccount is null || senderAccount is null)
            {
                return Result<MessageResponseDTO>.Failure("One or more participant does not exist.", StatusCodes.Status404NotFound);
            }
            var newRoom = new ChatRoom();
            newRoom.Participants.Add(new RoomParticipant { UserId = request.UserId });
            newRoom.Participants.Add(new RoomParticipant { UserId = targetReceiverId!.Value });
            await _db.Chatrooms.AddAsync(newRoom);
            await _db.SaveChangesAsync(cancellation);

            finalRoomId = newRoom.Id;
            finalRecipientId = targetReceiverId.Value;
            Username = senderAccount.name;
        }
        var newMessage = new ChatMessage
        {
            RoomId = finalRoomId,
            SenderId = request.UserId,
            MessageText = request.MessageData.Message,
            TimeStamp = DateTime.UtcNow
        };

        await _db.Messages.AddAsync(newMessage);
        await _db.SaveChangesAsync(cancellation);
        string newMessageHashedId = _hasher.CreateHashids(newMessage.Id, HashContext.Message);
        string hashedRoomId = _hasher.CreateHashids(finalRoomId, HashContext.Room);
        string hashedRecipientId = _hasher.CreateHashids(finalRecipientId, HashContext.User);

    return Result<MessageResponseDTO>.Success(
        new MessageResponseDTO
            (newMessageHashedId, hashedRoomId, Username, request.MessageData.Message, newMessage.TimeStamp.ToString(), hashedRecipientId)
        );
    }
}