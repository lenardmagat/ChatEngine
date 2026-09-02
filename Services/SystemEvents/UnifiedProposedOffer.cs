using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsOfferMatch;
using ChatSystem.PipeLine.IsProductExisting;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedProposedMechanism;
public class UnifiedOffer
{   z
    public record ProposedCommand: IRequest<Result<MessageResponseDTO>>, IExistingCommandAndMatch, IMatchOfferToProduct
    {
        public int UserId {get; set;}
        public ProposedItemDTO ProposedData {get; set;} = null!;
        public string ResourceId => ProposedData.ItemId;
        public OfferTye Status => ProposedData.Offer;
    }
    public class Handler(
        IEnumerable<IProposedOfferStrategy> strategies,
        ILogger<Handler> logger
    ) : IRequestHandler<ProposedCommand, Result<MessageResponseDTO>>
    {
        private readonly Dictionary<OfferTye, IProposedOfferStrategy> _strategyMap = 
            strategies.ToDictionary(s => s.Target);
        public async Task<Result<MessageResponseDTO>> Handle(ProposedCommand command, CancellationToken cancellationToken)
        {   
            try{
                
                var req = command.ProposedData;
                if(!_strategyMap.TryGetValue(req.Offer, out var strategy))
                {
                    return Result<MessageResponseDTO>.Failure($"Invalid request.", StatusCodes.Status400BadRequest);
                }
                var result = await strategy.ProposedStrategy(command.UserId, req, cancellationToken);
                if(!result.IsSuccess) return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
                return result;
            }
            catch(Exception e)
            {
                logger.LogError(e, $"An unexpected error occured while handling ProposedCommand. Details : {command}");
                return Result<MessageResponseDTO>.Failure("An unexpected error occured in our server.", StatusCodes.Status500InternalServerError);
            }
        }
    }
    
}