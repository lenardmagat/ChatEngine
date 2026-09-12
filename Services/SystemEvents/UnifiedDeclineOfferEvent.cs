using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsOfferExisting;
using ChatSystem.PipeLine.IsProductExisting;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedDeclineMechanism;

public class UnifiedDeclineOffer
{
    public record DeclineOfferCommand : IRequest<Result<MessageResponseDTO>>, IExistingCommandAndMatch, IOfferExist
    {
        public int UserId { get; set; }
        public DeclineItemDTO ItemDTO { get; set; } = null!;
        public string ResourceId => ItemDTO.ItemId;
        public OfferTye Status => ItemDTO.OfferType;
        public string ParentOfferId => ItemDTO.ParentOfferId;
    }

    public class Handler(
        IEnumerable<IDeclineOfferStrategy> strategies,
        ILogger<Handler> logger
    ) : IRequestHandler<DeclineOfferCommand, Result<MessageResponseDTO>>
    {
        private readonly Dictionary<OfferTye, IDeclineOfferStrategy> _strategies
            = strategies.ToDictionary(s => s.Target);

        public async Task<Result<MessageResponseDTO>> Handle(DeclineOfferCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (!_strategies.TryGetValue(command.Status, out var strategy))
                {
                    logger.LogError($"User tried to access unregistered decline strategy! Details: {command}");
                    return Result<MessageResponseDTO>.Failure($"No decline strategy registered for {command.Status}", StatusCodes.Status400BadRequest);
                }
                var result = await strategy.DeclineStrategy(command.UserId, command.ItemDTO, cancellationToken);
                if (!result.IsSuccess) return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unexpected error occurred while handling DeclineOfferCommand. Details: {command}");
                return Result<MessageResponseDTO>.Failure("An unexpected error occurred in our server.", StatusCodes.Status500InternalServerError);
            }
        }
    }
}
