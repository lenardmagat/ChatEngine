using ChatSystem.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Serilog;

namespace ChatEngine.TestFactory;

public class ChatEngineTestFactory : WebApplicationFactory<Program>
{
    private Respawner _respawner = null!;
    private const string ConnString = "Host=localhost;Port=5433;Database=chat_system_db_test;Username=lenard_admin;Password=super_secure_password123";

    public ChatEngineTestFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        var testConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(testConfigPath, optional: false)
            .Build();

        foreach (var entry in config.AsEnumerable())
        {
            if (entry.Value != null)
            {
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
                Environment.SetEnvironmentVariable(entry.Key.Replace(":", "__"), entry.Value);
            }
        }
    }

    public async Task ResetDatabaseAsync()
    {
        using var conn = new NpgsqlConnection(ConnString);
        await conn.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
        await _respawner.ResetAsync(conn);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var testConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(testConfigPath, optional: false)
            .Build();

        foreach (var entry in config.AsEnumerable())
        {
            if (entry.Value != null)
            {
                builder.UseSetting(entry.Key, entry.Value);
            }
        }

        builder.ConfigureAppConfiguration((ctx, configBuilder) =>
        {
            configBuilder.AddJsonFile(testConfigPath, optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(Program).Assembly);
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DbManager>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<DbManager>(options =>
                options.UseNpgsql(ConnString));
            services.AddSerilog((services, config) =>
            {
                config.MinimumLevel.Error()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("Logs/chat_log-.txt", rollingInterval: RollingInterval.Day);
            });
        });
    }
}
