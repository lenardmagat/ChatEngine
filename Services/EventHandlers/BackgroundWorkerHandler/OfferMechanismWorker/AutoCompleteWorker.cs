using System.Data;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.SystemEvents.OfferBackgroundEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.EventHandler.OfferMechanisBackgroundWorker;
public class AutoCompleteOfferHander(DbManager db, ILogger<AutoCompleteOfferHander> logger, IHasher hasher) : IRequestHandler<AutoCompleteCommand, Result<MessageResponseDTO>>
{
    public async Task<Result<MessageResponseDTO>> Handle(AutoCompleteCommand command, CancellationToken cancellation)
    {
        var data = command.value;
        using var transaction = await db.Database.BeginTransactionAsync(cancellation);
        try
        {
            if(data.Type == OfferTye.Sale)
            {
                var offer = await db.SaleOffers
                    .Where(o => o.Id == data.OfferId)
                    .FirstOrDefaultAsync(cancellation);

                var prevStatus = offer!.Status;
                offer.Status = SaleOfferStatus.Completed;
                offer.Version += 1;

                var SaleEventOffer = new SaleOfferEvent
                {
                    SaleOfferId = offer.Id,
                    Version = offer.Version,
                    FromStatus = prevStatus,
                    ToStatus = offer.Status,
                    PricePerUnit = offer.PricePerUnit,
                    QuantityRequested = offer.QuantityRequested,
                    ActorUserId = db.SystemActor.UserId
                };
                await db.SaleOfferEvents.AddAsync(SaleEventOffer, cancellation);
                await db.SaveChangesAsync(cancellation);

                ChatMessage message = new ChatMessage
                {
                    RoomId = offer.RoomId,
                    SenderId = db.SystemActor.UserId,
                    Type = MessageType.System,
                    MessageText = "Offer automatically completed!",
                    SaleOfferEventId = (int)SaleEventOffer.Id,
                    SaleOfferId = offer.Id,
                };
                await db.Messages.AddAsync(message, cancellation);
                await db.SaveChangesAsync(cancellation);

                var responseMessage = await db.Messages
                    .AsNoTracking()
                    .Where(m => m.RoomId == offer.RoomId)
                    .Select(d => new
                        {
                            ParticipantsId = d.Room.Participants.Select(p => p.UserId).ToList(),
                            offerId = d.SaleOfferEvent.Id,
                            Itemid = offer.ItemId,
                            itemName= d.SaleOffer!.ItemDetails.ProductName,
                            proposedByusername = d.SaleOfferEvent.Actor.Username
                        }
                    ).FirstAsync(cancellation);
                MessageResponseDTO responseDTO = new MessageResponseDTO(
                
                    hasher.CreateHashids(offer.RoomId, HashContext.Room),
                    hasher.CreateHashids(responseMessage.ParticipantsId.First(), HashContext.User),
                    new MessageData(
                    
                        hasher.CreateHashids(message.Id, HashContext.Message),
                        message.MessageText,
                        message.TimeStamp,
                        db.SystemActor.Username,
                        hasher.CreateHashids(db.SystemActor.UserId, HashContext.User),
                        new SaleOfferResponseDTO(
                            hasher.CreateHashids((int)responseMessage.offerId, HashContext.SaleOffer),
                            hasher.CreateHashids(offer.ItemId, HashContext.Product),
                            responseMessage.itemName,
                            offer.QuantityRequested,
                            offer.ProposedByUserId,
                            offer.QuantityRequested * offer.ProposedByUserId,
                            offer.Status.ToString(),
                            responseMessage.proposedByusername,
                            SaleEventOffer.CreatedAt
                            ),
                        message.Type,
                        OfferTye.Sale
                    )
                );
                
                await transaction.CommitAsync(cancellation);
                return Result<MessageResponseDTO>.Success(responseDTO);
            }
            else
            {
                return Result<MessageResponseDTO>.Failure("No strategy yet implement to process request.", StatusCodes.Status405MethodNotAllowed);
            }
        }
        catch(DBConcurrencyException DbE)
        {
            await transaction.RollbackAsync(cancellation);
            logger.LogError(DbE, $"worker unsuccessfully tried to autocomplete offer. Details: {command}");
             return Result<MessageResponseDTO>.Failure("Auto-Complete worker Unsuccessfully to complete offer. ", StatusCodes.Status409Conflict);
        }
        catch(Exception e)
        {
            await transaction.RollbackAsync(cancellation);   
            logger.LogError(e, $"Something went wrong while processing AutoComplete offer. Details: {command}");
            return Result<MessageResponseDTO>.Failure("Auto-Complete worker Unsuccessfully to complete offer. ", StatusCodes.Status400BadRequest);
        }
    }
}