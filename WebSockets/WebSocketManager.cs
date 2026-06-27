using System.Collections.Concurrent;
using System.Net.WebSockets;
using ChatSystem.ErrorHandling;
namespace ChatSystem.WebSocketManger;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    public void AddUser(string userId, WebSocket socket) => _sockets[userId] = socket;

    public void RemoveUser(string userId) => _sockets.TryRemove(userId, out _);
    public Result<WebSocket> GetSocketByUserId(string userId)
    {
        _sockets.TryGetValue(userId, out var socket);
        if(socket is null) return Result<WebSocket>.Failure("Invalid credentials", 404);
        return Result<WebSocket>.Success(socket);
    }

}