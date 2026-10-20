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
using ChatEngine.Tests.AccountDatas;
using Microsoft.VisualBasic;
using System.Reflection;
namespace ChatEngine.Tests.AccountTests;
[Collection("ChatEngineTest")]
public class AccountTest : CoreIntegrationTestBase
{
    protected readonly AuthenticatedUserTestServices _authService;
    public AccountTest(ChatEngineTestFactory factory) : base(factory)
    {
        _authService = new AuthenticatedUserTestServices(factory);
    }

    [Theory]
    [MemberData(nameof(AccountTestDatas.CreateAccountData), MemberType = typeof(AccountTestDatas))]
    public async Task Register_AccountRouter_Test(string username, string password, int expectedStatusCode) 
    {
        var response = await _authService.Register(username, password);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(expectedStatusCode == (int)response.StatusCode, $"Expected {expectedStatusCode} but got {(int)response.StatusCode}. Body: {body}");
    }
    
    [Theory]
    [MemberData(nameof(AccountTestDatas.LoginAccountData), MemberType = typeof(AccountTestDatas))]
    public async Task Login_AccountRouter_Test(string username, string password)
    {
        var registerResponse = await _authService.RegisterAndLogin(username, password);
        Assert.True(typeof(string) == registerResponse.GetType(), registerResponse.ToString());
    }
    
    [Theory]
    [MemberData(nameof(AccountTestDatas.ChangePasswordData), MemberType = typeof(AccountTestDatas))]
    
    public async Task ChangePassword_AccountRouter_Test(string username, 
        string password, 
        string newPassword, 
        int expectedStatusCode, 
        string? OldPasswordInvalidation = null,
        string? JWTInvalidation = null
    )
    {
        var TestUserToken = await _authService.RegisterAndLogin(username, password);
        var changePasswordResponse = await _authService.ChangePassword(password + OldPasswordInvalidation, newPassword, TestUserToken + JWTInvalidation);
        var body = await changePasswordResponse.Content.ReadAsStringAsync();
        Assert.True(expectedStatusCode == (int)changePasswordResponse.StatusCode, $"Expected {expectedStatusCode}, but got {(int)changePasswordResponse.StatusCode}. Body: {body}");
    }

}