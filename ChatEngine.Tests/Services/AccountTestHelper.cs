using System.Net.Http.Json;
using ChatEngine.TestFactory;
using ChatSystem.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Headers;
namespace ChatEngine.Tests.Services;

public class AuthenticatedUserTestServices
{
    private readonly ChatEngineTestFactory _factory;
    private readonly HttpClient _client;

    public AuthenticatedUserTestServices(ChatEngineTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task<HttpResponseMessage> Register(string username, string password)
    {
        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(username), "Username");
        formData.Add(new StringContent(password), "password");

        return await _client.PostAsync("/API/Account/Create", formData); 
    }
    
    public async Task<string> RegisterAndLogin(string username, string password)
    {
        var registerResponse = await Register(username, password);
        registerResponse.EnsureSuccessStatusCode();

        var loginPayload = new
        {
            Username = username,
            password = password
        };
        var response = await _client.PostAsJsonAsync("/API/Account/Login", loginPayload);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<AuthJWTResponse>();
        return token?.AccessToken ?? string.Empty;
    }

    public async Task<HttpResponseMessage> ChangePassword(string oldPassword, string newPassword, string Jwt)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Jwt.Replace("\"", "").Trim());
        var ChangePasswordPayload = new
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        };
        return await _client.PatchAsJsonAsync("/API/Account/AccountUpdate-ChangePassword", ChangePasswordPayload);
    }

    public static async Task<HubConnection> ConnectHubAsync(ChatEngineTestFactory factory, string jwt)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"{factory.Server.BaseAddress}ChatHub?access_token={jwt}", options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        return connection;
    }
}
