using ChatSystem.WebSocketManger;
using ChatSystem.WebSocketServices;
using Microsoft.EntityFrameworkCore;
using ChatSystem.DataBase;
using Microsoft.IdentityModel.Protocols.Configuration;
using ChatSystem.Services;
using HashidsNet;
using ChatSystem.core;
namespace ChatSystem.Injection;
public static class DependenciesInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var _DbKey = configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if(string.IsNullOrEmpty(_DbKey)) throw new InvalidConfigurationException("Data Base connection string is misisng");
        services.AddDbContext<DbManager>(options => options.UseNpgsql(_DbKey));
        services.AddSingleton<WebSocketConnectionManager>();
        services.AddScoped<WebSocketHandler>();
        services.AddScoped<IChatServices, ChatServices>();
        services.AddScoped<IAccountServices, AccountServices>();
        services.AddSingleton<IHashids>(_ => new Hashids(configuration["Hashids:Salt"], 8));
        services.AddSingleton<IHasher>(sp =>
        {   var hashids = sp.GetRequiredService<IHashids>();
            string? keystring = configuration["Jwt:Key"] ?? throw new InvalidConfigurationException("JWT key string is missing.");
            string? issuer = configuration["Jwt:Issuer"] ?? throw new InvalidConfigurationException("Issuer key string is missing.");
            string? audience = configuration["Jwt:Audience"] ?? throw new InvalidConfigurationException("Audience key string is missing.");
            return new SystemSecurity(hashids, keystring, issuer, audience);      
        }
        );
        return services;
    }
}