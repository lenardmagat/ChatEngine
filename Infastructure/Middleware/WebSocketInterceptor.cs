using System.Net.WebSockets;
using System.Security.Claims;
using ChatSystem.WebSocketServices;
using ChatSystem.Extensions;
namespace ChatSystem.WebSocketMiddleware;
public class WebSocketRoutingMiddleware
{
    private readonly RequestDelegate _next;
    public WebSocketRoutingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, WebSocketHandler socketHandler)
    {
        if(context.Request.Path == "/ws/chat")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                if(context.User.Identity?.IsAuthenticated != true)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                int? UserId = context.User.GetUserId();
                if(UserId == null){
                    context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                    return;
                    }
                using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await socketHandler.HandleConnection(context, webSocket, UserId.Value);
                return;
            }
        }
        else
        {
            await _next(context);
        }
    }

}