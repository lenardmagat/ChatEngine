
using Microsoft.AspNetCore.SignalR;
namespace ChatSystem.HubExcept;
public class HubExceptionFilter : IHubFilter
{
    private readonly ILogger<HubExceptionFilter> _logger;
    public HubExceptionFilter(ILogger<HubExceptionFilter> logger) => _logger = logger;

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
    {
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