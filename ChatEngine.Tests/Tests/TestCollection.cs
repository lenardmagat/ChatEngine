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
namespace ChatEngine.Tests.collections;
[CollectionDefinition("ChatEngineTest")]
public class TestCollection : ICollectionFixture<ChatEngineTestFactory>, IAsyncLifetime
{
    private readonly ChatEngineTestFactory _factory;
    public TestCollection(ChatEngineTestFactory factory)
    {
        _factory = factory;
    }
    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();

        using var scope = _factory.Services.CreateScope();
        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        await InfraHelper.ClearS3BucketAsync(s3, "marketplace-test");

        var meiliClient = new HttpClient { BaseAddress = new Uri("http://localhost:7701") };
        meiliClient.DefaultRequestHeaders.Authorization = new("Bearer", "masterKey123_ChangeMeInProd!");
        await InfraHelper.ClearMeiliSearchAsync(meiliClient);
    }
    public Task DisposeAsync() => Task.CompletedTask;
}