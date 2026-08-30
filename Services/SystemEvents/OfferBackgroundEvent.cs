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