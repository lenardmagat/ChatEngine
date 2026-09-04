using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatSystem.DataBase;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DbManager>
{
    public DbManager CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();
        var optionsBuilder = new DbContextOptionsBuilder<DbManager>();
        
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration["ConnectionStrings__DefaultConnection"]
            ?? configuration["DB_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Could not find 'DefaultConnection' in appsettings.json or environment variables during design-time migration.");
        }

        if (connectionString.Contains("chat-db", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = connectionString
                .Replace("Host=chat-db", "Host=localhost", StringComparison.OrdinalIgnoreCase)
                .Replace("Port=5432", "Port=5433", StringComparison.OrdinalIgnoreCase);
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new DbManager(optionsBuilder.Options);
    }
}