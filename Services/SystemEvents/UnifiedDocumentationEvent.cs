using ChatSystem.DTOs.Documentation;
using ChatSystem.ErrorHandling;
using ChatSystem.Services.Interfaces;
using MediatR;
namespace ChatSystem.SystemEvents.Documentation;
public class UnifiedDocument{
    public record DocumentationCommand(DocumentRequest request) : IRequest<Result>;
    public class Handler(
        IEnumerable<IDocumentStrategy> strategies,
        ILogger<Handler> logger
        ) 
            : IRequestHandler<DocumentationCommand, Result>
    {
        private readonly Dictionary<DocumentTarget, IDocumentStrategy> _strategies
            = strategies.ToDictionary(s => s.Target);
        public async Task<Result> Handle(DocumentationCommand document, CancellationToken cancellation = default)
        {
            try
            {
                var req = document.request;
                if(!_strategies.TryGetValue(req.Target, out var strategy))
                {
                    return Result.Failure($"No Document strategy registered for {req.Target}", StatusCodes.Status400BadRequest);
                }
                await strategy.DocumentAsync(document.request, cancellation);
                return Result.Success();
            }catch(Exception e)
            {
                logger.LogError(e, "Error occurred while handling document request for target {Target}", document.request.Target);
                return Result.Failure("An Unexpected Internal Server Occured", StatusCodes.Status500InternalServerError);
            }
        }
    }
}