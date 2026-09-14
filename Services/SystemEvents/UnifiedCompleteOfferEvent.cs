using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsOfferExisting;
using ChatSystem.PipeLine.IsProductExisting;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedChat;
public class UnifiedCompleteOffer
{
    public record CompleteOfferCommand(
        int UserId,
        CompleteOfferDTO CompleteOffer
    ) : IRequest<Result<MessageResponseDTO>>, IOfferExist
    {
        public string ParentOfferId => CompleteOffer.ParentOfferId;
        public OfferTye Status => CompleteOffer.OfferType;
    }
    public class Handler(
        IEnumerable<ICompleteStrategy> strategies,
        ILogger<Handler> logger
    ) : IRequestHandler<CompleteOfferCommand, Result<MessageResponseDTO>>
    {
        private readonly Dictionary<OfferTye, ICompleteStrategy> _strategies
            = strategies.ToDictionary(s => s.Target);
        public async Task<Result<MessageResponseDTO>> Handle(CompleteOfferCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                var req = command.CompleteOffer;
                if(!_strategies.TryGetValue(req.OfferType, out var strategy))
                {
                    return Result<MessageResponseDTO>.Failure($"No Complete Offer strategy registered for {req.OfferType}", StatusCodes.Status400BadRequest);
                }
                return await strategy.CompleteStrategy(command.UserId, req, cancellationToken);
            }catch(Exception e)
            {
                logger.LogError(e, "Error occurred while handling complete offer request for target {Target}", command.CompleteOffer.OfferType);
                return Result<MessageResponseDTO>.Failure("An Unexpected Internal Server Occured", StatusCodes.Status500InternalServerError);
            }
        }
    }
}