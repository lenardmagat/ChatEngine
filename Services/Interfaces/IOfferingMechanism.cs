using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsProductExisting;

namespace ChatSystem.Services.Interfaces.OfferingMechanism;
public interface IProposedOfferStrategy
{
    OfferTye Target {get;}
    Task<Result<MessageResponseDTO>> ProposedStrategy(int UserId, ProposedItemDTO data, CancellationToken cancellationToken);
}
public interface IAcceptOfferStrategy
{
    OfferTye Target {get;}
    Task<Result<MessageResponseDTO>> AcceptStrategy(int UserId, AcceptItemDTO data, CancellationToken cancellationToken);
}

