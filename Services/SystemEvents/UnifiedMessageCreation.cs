using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using ChatSystem.Services.Interfaces;
using ChatSystem.SystemEvents.Chats;
using MediatR;

namespace ChatSystem.SystemEvents.UnifiedChat;
public static class UnifiedChat
{
    public record MessageCommand(int UserId, SendMessage Request) : IRequest<Result<MessageResponseDTO>>;
    public class Handler(
        IEnumerable<IMessageStrategy> strategies,
        ILogger<Handler> logger)
            : IRequestHandler<MessageCommand, Result<MessageResponseDTO>>
    {
        private readonly Dictionary<MessageType, IMessageStrategy> _strategyMap = 
            strategies.ToDictionary(s => s.Target);
        public async Task<Result<MessageResponseDTO>> Handle(MessageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var req = command.Request;
                if(!_strategyMap.TryGetValue(req.Type, out var strategy))
                {
                    logger.LogError($"No Strategy registered for {req.Type}. Details{command}");
                    return Result<MessageResponseDTO>.Failure("Invalid payload", StatusCodes.Status400BadRequest);
                }
                var result = await strategy.MessageHandler(command.UserId, command.Request, cancellationToken);
                if(!result.IsSuccess) return Result<MessageResponseDTO>.Failure(result.Error!, result.StatusCode);
                return Result<MessageResponseDTO>.Success(result.Value!);
            }
            catch(Exception e)
            {
                logger.LogError(e, $"An unexpected error occured while processing MessageCommand. Details: {command}");
                return Result<MessageResponseDTO>.Failure("An unexpected error occured in our server.", StatusCodes.Status500InternalServerError);
            }
        }
    }
}