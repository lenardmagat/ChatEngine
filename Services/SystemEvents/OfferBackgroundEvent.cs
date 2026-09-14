using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using MediatR;

namespace ChatSystem.SystemEvents.OfferBackgroundEvents;
public record ExpiredOfferDTO
(
    int Itemid,
    OfferTye Type
);
public record ExpiredOfferCommand(ExpiredOfferDTO Value) : IRequest<Result>; 
public record CompletedOfferDTO(
    int OfferId,
    OfferTye Type
);
public record AutoCompleteCommand(CompletedOfferDTO value) :  IRequest<Result<MessageResponseDTO>>;