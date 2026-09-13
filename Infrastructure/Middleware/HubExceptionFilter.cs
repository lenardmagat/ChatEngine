
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR;
namespace ChatSystem.HubExcept;
public class HubExceptionFilter : IHubFilter
{
    private readonly ILogger<HubExceptionFilter> _logger;
    public HubExceptionFilter(ILogger<HubExceptionFilter> logger) => _logger = logger;

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var expClaim = context.Context.User?.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (expClaim is not null && long.TryParse(expClaim, out var expUnix))
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            if (expiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Rejected hub call to {Method}: token expired at {ExpiresAt}", 
                    context.HubMethodName, expiresAt);
                context.Context.Abort();
                throw new HubException("Session expired. Please reconnect.");
            }
        }
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in hub method {Method}", context.HubMethodName);
            throw new HubException("Something went wrong processing your request."); // safe message to client
        }
    }
}