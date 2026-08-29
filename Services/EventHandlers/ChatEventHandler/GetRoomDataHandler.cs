using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.Models;
using Serilog;

namespace ChatSystem.EventHandler.Chats;
public class GetRoomDataHandler : IRequestHandler<GetRoomDataCommand, Result<RoomDataDTO>>
{
    private readonly DbManager _db;
    private readonly IHasher _hasher;
    private readonly ILogger<GetRoomDataHandler> _logger;
    public GetRoomDataHandler(DbManager db, IHasher hasher, ILogger<GetRoomDataHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }
    public async Task<Result<RoomDataDTO>> Handle(GetRoomDataCommand request, CancellationToken cancellationToken)
    {
        Result<int>? decodedRoomId = !string.IsNullOrEmpty(request.RoomId)
            ? _hasher.DecodeOrFail(request.RoomId, HashContext.Room) : null;
        Result<int>? decodedRecepientId = !string.IsNullOrEmpty(request.ReceiverId)
            ? _hasher.DecodeOrFail(request.ReceiverId, HashContext.User) : null;
        if (decodedRoomId is null && decodedRecepientId is null)
            return Result<RoomDataDTO>.Failure("Either a room or a receiver must be specified.", StatusCodes.Status400BadRequest);
        if (decodedRoomId is not null && !decodedRoomId.IsSuccess)
            return Result<RoomDataDTO>.Failure(decodedRoomId.Error!, decodedRoomId.StatusCode);
        if (decodedRecepientId is not null && !decodedRecepientId.IsSuccess)
            return Result<RoomDataDTO>.Failure(decodedRecepientId.Error!, decodedRecepientId.StatusCode);
        int? targetRoomId = decodedRoomId?.Value;
        int? targetReceiverId = decodedRecepientId?.Value;

        var roomData = await _db.Chatrooms
            .AsNoTracking()
            .Where(r => targetRoomId.HasValue 
                ? r.Id == targetRoomId.Value 
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
            .FirstOrDefaultAsync(cancellationToken);
        RoomDataDTO roomDataDTO;
        if (roomData != null)
        {
            if (!roomData.IsSenderParticipant)
            {
                return Result<RoomDataDTO>.Failure("You do not have permission to post here.", StatusCodes.Status403Forbidden);
            }
            roomDataDTO = new RoomDataDTO(roomData.RoomId, roomData.RecipientUserId, roomData.SenderName!);
        }
        else
        {
            if (targetRoomId.HasValue)
            {
            return Result<RoomDataDTO>.Failure("Chat session not found.", StatusCodes.Status404NotFound);
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
                return Result<RoomDataDTO>.Failure("One or more participant does not exist.", StatusCodes.Status404NotFound);
            }
            var newRoom = new ChatRoom();
            newRoom.Participants.Add(new RoomParticipant { UserId = request.UserId });
            newRoom.Participants.Add(new RoomParticipant { UserId = targetReceiverId!.Value });
            await _db.Chatrooms.AddAsync(newRoom);
            await _db.SaveChangesAsync(cancellationToken);
            roomDataDTO = new RoomDataDTO(newRoom.Id, targetReceiverId.Value, senderAccount.name);
        }
        return Result<RoomDataDTO>.Success(roomDataDTO);
    }
           
}