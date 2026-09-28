using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using System.Linq.Expressions;
using ChatSystem.SystemEvents.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChatSystem.core;
namespace ChatSystem.EventHandler.Chats;
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
        Result<int>? decodedRoomId = !string.IsNullOrEmpty(command.ChatId) 
            ? _hasher.DecodeOrFail(command.ChatId, HashContext.Room) 
            : null;
        if (decodedRoomId is not null && !decodedRoomId.IsSuccess)
        {
            return Result<ChatData?>.Failure(decodedRoomId.Error!, decodedRoomId.StatusCode);
        }

        Result<int>? decodedReceiverId = !string.IsNullOrEmpty(command.RecieverId) 
            ? _hasher.DecodeOrFail(command.RecieverId, HashContext.User) 
            : null;
        if (decodedReceiverId is not null && !decodedReceiverId.IsSuccess)
        {
            return Result<ChatData?>.Failure(decodedReceiverId.Error!, decodedReceiverId.StatusCode);
        }

        int? targetRoomId = decodedRoomId?.Value;
        int? targetReceiverID = decodedReceiverId?.Value;

        var query = _db.Chatrooms.AsNoTracking();
        if (targetRoomId.HasValue)
        {
            query = query.Where(r => r.Id == targetRoomId.Value && r.Participants.Any(p => p.UserId == command.UserId));
        }
        else if (targetReceiverID.HasValue)
        {
            query = query
                .Where(c => 
                    c.Participants.Any(p => p.UserId == command.UserId) &&
                    c.Participants.Any(p => p.UserId == targetReceiverID.Value)
                    );
        }
        else
        {
            return Result<ChatData?>.Failure("Either ChatId or RecieverId must be specified.", StatusCodes.Status400BadRequest);
        }

        var ChatDataProjection = await query
            .Select(r => new
            {
                RoomId = r.Id,
                ReceiverId = r.Participants
                    .Where(p => p.UserId != command.UserId)
                    .Select(u => u.UserId)
                    .FirstOrDefault(),
                LastMessageTimeStampt = r.Messages
                    .Max(m => m.TimeStamp),
                RecentMessages = r.Messages
                    .OrderByDescending(m => m.Id)
                    .Take(10)
                    .AsQueryable()
                    .Select(m => new
                    {
                        m.Id,
                        m.MessageText,
                        m.TimeStamp,
                        m.Sender.Username,
                        m.SenderId,
                        SaleOfferEvent = m.SaleOfferEvent == null ? null : new
                        {
                            SaleOfferId = m.SaleOfferEvent.SaleOfferId,
                            ItemId = m.SaleOfferEvent.SaleOffer.ItemId,
                            ItemName = m.SaleOfferEvent.SaleOffer.ItemDetails.ProductName,
                            QuantityRequested = m.SaleOfferEvent.QuantityRequested,
                            PricePerUnit = m.SaleOfferEvent.PricePerUnit,
                            Status = m.SaleOfferEvent.ToStatus,
                            ProposedByUsername = m.SaleOfferEvent.Actor.Username,
                            CreatedAt = m.SaleOfferEvent.CreatedAt
                        },
                        SaleOffer = m.SaleOffer == null ? null : new
                        {
                            Id = m.SaleOffer.Id,
                            ItemId = m.SaleOffer.ItemId,
                            ItemName = m.SaleOffer.ItemDetails.ProductName,
                            UserProposedUsername = m.SaleOffer.UserProposed.Username,
                            PricePerUnit = m.SaleOffer.PricePerUnit,
                            QuantityRequested = m.SaleOffer.QuantityRequested,
                            Status = m.SaleOffer.Status,
                            CreatedAt = m.SaleOffer.CreatedAt
                        },
                        TradeOffer = m.TradeOffer == null ? null : new TradeOffer
                        {
                            Id = m.TradeOffer.Id,
                            ItemRequestedId = m.TradeOffer.ItemRequestedId,
                            ItemOffered = m.TradeOffer.ItemOffered,
                            Status = m.TradeOffer.Status,
                            CreatedAt = m.TradeOffer.CreatedAt
                        },
                        Type = m.Type
                    })
                    .ToList()
            }
            ).FirstOrDefaultAsync(cancellation);
        if(ChatDataProjection is null)
        {
            if (targetRoomId.HasValue)
            {
                bool roomExists = await _db.Chatrooms.AnyAsync(r => r.Id == targetRoomId.Value, cancellation);
                if (roomExists)
                {
                    return Result<ChatData?>.Failure("You do not have permission to access this chat room.", StatusCodes.Status403Forbidden);
                }
                return Result<ChatData?>.Failure("Chat room session no longer exists.", StatusCodes.Status404NotFound);
            }
            return Result<ChatData?>.Success(new ChatData(true, null ,null ,null ,null));
        }
        List<MessageData> messageDatas = ChatDataProjection
            .RecentMessages
            .Select(m =>
            {
                SaleOfferResponseDTO? saleOfferDTO = null;
                if (m.SaleOfferEvent is not null)
                {
                    saleOfferDTO = new SaleOfferResponseDTO(
                        _hasher.CreateHashids(m.SaleOfferEvent.SaleOfferId, HashContext.SaleOffer),
                        _hasher.CreateHashids(m.SaleOfferEvent.ItemId, HashContext.Product),
                        m.SaleOfferEvent.ItemName,
                        m.SaleOfferEvent.QuantityRequested,
                        m.SaleOfferEvent.PricePerUnit,
                        m.SaleOfferEvent.PricePerUnit * m.SaleOfferEvent.QuantityRequested,
                        m.SaleOfferEvent.Status.ToString(),
                        m.SaleOfferEvent.ProposedByUsername,
                        m.SaleOfferEvent.CreatedAt
                    );
                }
                else if (m.SaleOffer is not null)
                {
                    saleOfferDTO = new SaleOfferResponseDTO(
                        _hasher.CreateHashids(m.SaleOffer.Id, HashContext.SaleOffer),
                        _hasher.CreateHashids(m.SaleOffer.ItemId, HashContext.Product),
                        m.SaleOffer.ItemName,
                        m.SaleOffer.QuantityRequested,
                        m.SaleOffer.PricePerUnit,
                        m.SaleOffer.PricePerUnit * m.SaleOffer.QuantityRequested,
                        m.SaleOffer.Status.ToString(),
                        m.SaleOffer.UserProposedUsername,
                        m.SaleOffer.CreatedAt
                    );
                }

                return new MessageData(
                    _hasher.CreateHashids(m.Id, HashContext.Message),
                    m.MessageText,
                    m.TimeStamp,
                    m.Username,
                    _hasher.CreateHashids(m.SenderId, HashContext.User),
                    saleOfferDTO,
                    m.Type
                );
            }).ToList();
        ChatData data = new ChatData(
            false,
            _hasher.CreateHashids(ChatDataProjection.RoomId, HashContext.Room),
            ChatDataProjection.LastMessageTimeStampt,
            _hasher.CreateHashids(ChatDataProjection.ReceiverId, HashContext.User),
            messageDatas
        );
        return Result<ChatData?>.Success(data);
    }
}