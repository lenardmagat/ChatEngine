using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;

namespace ChatSystem.Services.Interfaces.OfferingMechanism;
public interface IProposedOfferStrategy
{
    OfferTye Target {get;}
    Task<Result<MessageResponseDTO>> ProposedStrategy(ProposedItemDTO data, CancellationToken cancellation);
}