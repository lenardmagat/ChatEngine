using ChatEngine.Tests.collections;
using ChatEngine.Tests.BaseTests;
using Amazon.S3;
using ChatEngine.InfrastructureDeletion;
using ChatEngine.TestFactory;
using ChatEngine.Tests.Services;
using ChatSystem.core;
using ChatSystem.DataBase;
using ChatSystem.DTOs;
using ChatSystem.ErrorHandling;
using ChatSystem.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
namespace ChatEngine.Tests.ChatFeature;
[Collection("ChatEngineTest")]
public class ChatFeatureTests : CoreIntegrationTestBase
{
    protected readonly AuthenticatedUserTestServices _authService;
    protected readonly ChatSocketTestHandler _chatSocketTestHandler;
    public ChatFeatureTests(ChatEngineTestFactory factory) : base(factory)
    {
        _authService = new AuthenticatedUserTestServices(factory);
        _chatSocketTestHandler = new ChatSocketTestHandler(factory);
    }
    
    [Fact]
    public async Task ConnectHub_WithValidTokens_Returns200()
    {
        var token = await _authService.RegisterAndLogin("testuser1", "TestPass123!");
        var connection = await _chatSocketTestHandler.ConnectToChatHubAsync(token);
        Assert.Equal(HubConnectionState.Connected, connection.State);
        await connection.DisposeAsync();
    }
    [Fact]
    public async Task ConnectHub_WithInvalidTokens_Returns401()
    {
        var token = "invalid_token";
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => 
            _chatSocketTestHandler.ConnectToChatHubAsync(token)
        );
        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task SendMessage_WithValidTokens_Returns200()
    {
        var BuyerToken = await _authService.RegisterAndLogin("sendMessageUser1", "TestPass123!");
        var SellerToken = await _authService.RegisterAndLogin("sendMessageUser2", "TestPass456!");
        var client = _factory.CreateClient();

        await using var BuyerConnection = await _chatSocketTestHandler.ConnectToChatHubAsync(BuyerToken);
        await using var SellerConnection = await _chatSocketTestHandler.ConnectToChatHubAsync(SellerToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbManager>();
        var hasher = scope.ServiceProvider.GetRequiredService<IHasher>();

        string sellerhashedId = hasher.CreateHashids(await db.Users.Where(u => u.Username == "sendMessageUser2").Select(u => u.UserId).FirstOrDefaultAsync(), HashContext.User);
        string buyerhashedId = hasher.CreateHashids(await db.Users.Where(u => u.Username == "sendMessageUser1").Select(u => u.UserId).FirstOrDefaultAsync(), HashContext.User);

        var sellerReceivedTcs = new TaskCompletionSource<MessageData>();
        var buyerReceivedTcs = new TaskCompletionSource<MessageData>();

        using var sellerSub = SellerConnection.On<MessageData>("NewMessage", msg =>
        {
            sellerReceivedTcs.TrySetResult(msg);
        });

        using var buyerSub = BuyerConnection.On<MessageData>("NewMessage", msg =>
        {
            buyerReceivedTcs.TrySetResult(msg);
        });

        await BuyerConnection.InvokeAsync("SendMessage", new SendMessage(
            RoomId: null,
            Message: "Hello, this is a test message for seller.",
            RecieverId: sellerhashedId,
            Type: MessageType.Text,
            OfferPayload: null
        ));

        await SellerConnection.InvokeAsync("SendMessage", new SendMessage(
            RoomId: null,
            Message: "Hello, this is a test message for buyer.",
            RecieverId: buyerhashedId,
            Type: MessageType.Text,
            OfferPayload: null
        ));

        var sellerResult = await sellerReceivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var buyerResult = await buyerReceivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal("Hello, this is a test message for buyer.", sellerResult.ChatMessage);
        Assert.Equal("Hello, this is a test message for seller.", buyerResult.ChatMessage);

        await BuyerConnection.DisposeAsync();
        await SellerConnection.DisposeAsync();
    }

    [Theory]
    [InlineData("testuser1", "WrongPass123!")]
    [InlineData("testuser2", "WrongPass123!")]
    [InlineData("testuser3", "WrongPass123!")]
    public async Task LoadChatHub_WithValidData_Returns200(string username, string password)
    {
        var token = await _authService.RegisterAndLogin(username, password);
        var connection = await AuthenticatedUserTestServices.ConnectHubAsync(_factory, token);
        
        var tcs = new TaskCompletionSource<List<LoadConversationResponse>>(); 
        
        using var userSub = connection.On<List<LoadConversationResponse>>("Load Conversation", data =>
        {
            tcs.TrySetResult(data);
        });
        
        await connection.InvokeAsync("LoadChats");
        var dataList = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.NotNull(dataList);
        Assert.Empty(dataList);
        
        await connection.DisposeAsync();
    }
}