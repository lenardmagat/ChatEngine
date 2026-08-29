using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsProductExisting;

namespace ChatSystem.Services.Interfaces.OfferingMechanism;
public interface IProposedOfferStrategy
{
    OfferTye Target {get;}
    Task<Result<MessageResponseDTO>> ProposedStrategy(int UserId, ProposedItemDTO data, CancellationToken cancellation);
}


