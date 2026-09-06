using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsOfferExisting;
using ChatSystem.PipeLine.IsProductExisting;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedCounterMechanism;
public class UnifiedCounterOffer
{
    public record CounterOfferCommand(
        int UserId,
        CounterOfferDTO OfferPayload
    ) : IRequest<Result<MessageResponseDTO>>, IExistingCommandAndMatch, IOfferExist
    {
        public string ResourceId => OfferPayload.ItemId;
        public OfferTye Status => OfferPayload.OfferType;
        public string ParentOfferId => OfferPayload.ParentOfferId;
    }
    public class Handler(
        IEnumerable<ICounterOfferStrategy> strategies,
        ILogger<Handler> logger
    ) : IRequestHandler<CounterOfferCommand, Result<MessageResponseDTO>>
    {
        private readonly Dictionary<OfferTye, ICounterOfferStrategy> _strategies
            = strategies.ToDictionary(s => s.Target);
        public async Task<Result<MessageResponseDTO>> Handle(CounterOfferCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var req = command.OfferPayload;
                if(!_strategies.TryGetValue(req.OfferType, out var strategy))
                {
                    return Result<MessageResponseDTO>.Failure($"No Counter Offer strategy registered for {req.OfferType}", StatusCodes.Status400BadRequest);
                }
                return await strategy.CounterOfferStrategy(command.UserId, req, cancellationToken);
            }catch(Exception e)
            {
                logger.LogError(e, "Error occurred while handling counter offer request for target {Target}", command.OfferPayload.OfferType);
                return Result<MessageResponseDTO>.Failure("An Unexpected Internal Server Occured", StatusCodes.Status500InternalServerError);
            }
        }
    }
}