using System.Net.Http.Json;
using ChatEngine.TestFactory;
using ChatSystem.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
namespace ChatEngine.Tests.Services;
public class ChatSocketTestHandler
{
    private readonly ChatEngineTestFactory _factory;
    private readonly HttpClient _client;

    public ChatSocketTestHandler(ChatEngineTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task<HubConnection> ConnectToChatHubAsync(string jwtToken)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"{_factory.Server.BaseAddress}ChatHub?access_token={jwtToken}", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        return connection;
    }

}