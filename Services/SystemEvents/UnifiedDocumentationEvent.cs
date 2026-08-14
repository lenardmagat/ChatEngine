using ChatSystem.DTOs.Documentation;
using ChatSystem.ErrorHandling;
using ChatSystem.Services.Interfaces;
using MediatR;
namespace ChatSystem.SystemEvents.Documentation;
public class UnifiedDocument
{
    public record DocumentationCommand(DocumentRequest request) : IRequest<Result>;
    public class Handler(IEnumerable<IDocumentStrategy> strategies) 
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
                return Result.Failure("Unexpected Error occured in the server", StatusCodes.Status500InternalServerError);
            }
        }
    }
}