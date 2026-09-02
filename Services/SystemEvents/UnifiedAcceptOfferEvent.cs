using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsProductExisting;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedAcceptMechanism;
public class UnifiedAcceptOffer
{
    public record AcceptOfferCommand : IRequest<Result<MessageResponseDTO>>, IExistingCommandAndMatch
    {
        public int UserId {get; set;}
        public AcceptItemDTO itemDTO{get; set;} = null!;
        public string ResourceId => itemDTO.ItemId;
        public OfferTye Status => itemDTO.OfferType;
    }
}