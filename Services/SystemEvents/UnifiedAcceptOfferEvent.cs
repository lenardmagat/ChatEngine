using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.PipeLine.IsOfferExisting;
using ChatSystem.PipeLine.IsProductExisting;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedAcceptMechanism;
public class UnifiedAcceptOffer
{
    public record AcceptOfferCommand : IRequest<Result<MessageResponseDTO>>, IExistingCommandAndMatch, IOfferExist
    {
        public int UserId {get; set;}
        public AcceptItemDTO itemDTO{get; set;} = null!;
        public string ResourceId => itemDTO.ItemId;
        public OfferTye Status => itemDTO.OfferType;
        public string ParentOfferId => itemDTO.ParentOfferId;
    }
    public class Handler(
        IEnumerable<IAcceptOfferStrategy> strategies,
        ILogger<Handler> logger
    ) : IRequestHandler<AcceptOfferCommand, Result<MessageResponseDTO>>
    {
        private Dictionary<OfferTye, IAcceptOfferStrategy> _strategies 
            => strategies.ToDictionary(s => s.Target);
        public async Task<Result<MessageResponseDTO>> Handle(AcceptOfferCommand command, CancellationToken cancellationToken)
        {
            try{
                if(!_strategies.TryGetValue(command.Status, out var strategy))
                {
                    logger.LogError($"User tried to access unregistered strategy! Details: {command}");
                    return Result<MessageResponseDTO>.Failure($"No Document strategy registered for {command.Status}", StatusCodes.Status400BadRequest);
                }
                var result = await strategy.AcceptStrategy(command.UserId, command.itemDTO, cancellationToken);
                if(!result.IsSuccess) return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
                return result;
            }catch(Exception e)
            {
                logger.LogError(e, $"Unexpected error occured in while handling AcceptOfferCommand. Details: {command}");
                return Result<MessageResponseDTO>.Failure("An unexpected error occured in our server.", StatusCodes.Status400BadRequest);
            }
        }
    }
}