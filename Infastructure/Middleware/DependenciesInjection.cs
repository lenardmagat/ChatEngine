
using Microsoft.EntityFrameworkCore;
using ChatSystem.DataBase;
using Microsoft.IdentityModel.Protocols.Configuration;
using ChatSystem.Services;
using HashidsNet;
using ChatSystem.core;
using ChatSystem.core.KeyConfiguration;
namespace ChatSystem.Injection;
public static class DependenciesInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var _DbKey = configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if(string.IsNullOrEmpty(_DbKey)) throw new InvalidConfigurationException("Data Base connection string is misisng");
        services.AddDbContext<DbManager>(options => options.UseNpgsql(_DbKey));
        services.AddScoped<IAccountServices, AccountServices>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddOptions<HashidsSettings>()
            .Bind(configuration.GetSection("Hashids"))
            .ValidateDataAnnotations()
            .ValidateOnStart(); 
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IHasher, SystemSecurity>();
        return services;
    }
}