using System.Net.WebSockets;
using System.Text;
using ChatSystem.DataBase;
using ChatSystem.Services;
using ChatSystem.WebSocketManger;
using System.Text.Json;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
namespace ChatSystem.WebSocketServices;
public class WebSocketHandler
{
    private readonly WebSocketConnectionManager _manager;
    private readonly IChatServices _chatServices;
    public WebSocketHandler(WebSocketConnectionManager manager, IChatServices chatServices, DbManager db, IHasher hasher){
        _manager = manager;
        _chatServices = chatServices;
    }

    public async Task HandleConnection(HttpContext context, WebSocket socket, int UserId)
    {
        _manager.AddUser(UserId.ToString(), socket);
        var buffer = new byte [1024 *4];
        try
            {
                while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if(result.MessageType == WebSocketMessageType.Close) break; 
                        if(result.MessageType == WebSocketMessageType.Text)
                        {
                            string jsonPayload = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            var messageRequest = JsonSerializer.Deserialize<SendMessage>(jsonPayload);
                            var response = await _chatServices.DirectMessage(UserId, messageRequest!);
                            if (!response.IsSuccess)
                            {
                                var errorPayload = new { error = "ValidationError", message = response.Error};
                                byte[] errorBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorPayload));
                                await socket.SendAsync(new ArraySegment<byte>(errorBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            else
                            {
                                Result<WebSocket> RecipientSocket = await Task.Run(() => _manager.GetSocketByUserId(messageRequest!.RecipientId));
                                if (RecipientSocket.IsSuccess)
                                {
                                    byte [] payloadBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response.Value));
                                    await RecipientSocket.Value!.SendAsync(new ArraySegment<byte>(payloadBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                                    
                            }
                            continue;
                        }
                    }
            }
        
        catch (WebSocketException ex)
        {
            Console.Write(ex);
        }
        finally
        {
            _manager.RemoveUser(UserId.ToString());
            if(socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }
    }
}
