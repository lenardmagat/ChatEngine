using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatSystem.DataBase;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DbManager>
{
    public DbManager CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbManager>();
        
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration["ConnectionStrings__DefaultConnection"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Could not find 'DefaultConnection' in appsettings.json or environment variables during design-time migration.");
        }
        optionsBuilder.UseNpgsql(connectionString);

        return new DbManager(optionsBuilder.Options);
    }
}