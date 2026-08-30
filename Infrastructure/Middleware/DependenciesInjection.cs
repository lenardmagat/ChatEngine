
using Microsoft.EntityFrameworkCore;
using ChatSystem.DataBase;
using Microsoft.IdentityModel.Protocols.Configuration;
using ChatSystem.Services;
using ChatSystem.core;
using ChatSystem.core.KeyConfiguration;
using Meilisearch;
using Microsoft.Extensions.Options;
using ChatSystem.Services.Interfaces;
using ChatSystem.EventHandler.Search;
using ChatSystem.EventHandler.Documentation;
using ChatSystem.BackgroundServices.MeiliSync;
using ChatSystem.core.Jwt;
using ChatSystem.Services.Auth.Jwt;
using MediatR;
using ChatSystem.SystemEvents.Inventory;
using ChatSystem.EventHandler.Chats;
using ChatSystem.Services.Interfaces.OfferingMechanism;
using ChatSystem.EventHandler.OfferingMechanism;
using ChatSystem.PipeLine.IsProductExisting;
using ChatSystem.BackgroundServices;
namespace ChatSystem.Injection;
public static class DependenciesInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var _DbKey = configuration.GetConnectionString("DefaultConnection") ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if(string.IsNullOrEmpty(_DbKey)) throw new InvalidConfigurationException("Data Base connection string is misisng");
        services.AddDbContext<DbManager>(options => options.UseNpgsql(_DbKey));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(OwnerShipAuthorizationBehaviour<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ProductExistingBehaviour<,>));
        services.AddOptions<HashidsSettings>()
            .Bind(configuration.GetSection("Hashids"))
            .ValidateDataAnnotations()
            .ValidateOnStart(); 
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IHasher, SystemSecurity>();
        services.AddOptions<MeiliSearchSettings>()
            .Bind(configuration.GetSection("MeiliSearch"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<MeilisearchClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MeiliSearchSettings>>().Value;
            return new MeilisearchClient(settings.Url, settings.MasterKey);
        });
        services.AddScoped<IDynamicSearchService, DynamicMeiliSearchService>(); 
        services.AddScoped<ISearchStrategy, UserSearchStrategy>();
        services.AddScoped<ISearchStrategy, ProductSearchStrategy>();
        services.AddScoped<IDocumentStrategy, UserDocumentationStrategy>();
        services.AddScoped<IDocumentStrategy, ProductDocumentationStrategy>();
        services.AddScoped<IMessageStrategy, SendMessageTextStrategy>();
        services.AddScoped<IMessageStrategy, SendMessageProposedStrategy>();
        services.AddScoped<IProposedOfferStrategy, SaleProposedHandler>();
        services.AddScoped<IJwtTokenServices, JwtServices>();
        services.AddScoped<JWTAuthServices>();
        services.AddScoped<IAuthServices, JWTAuthServices>();
        services.AddHostedService<MeiliSyncWorker>();
        services.AddHostedService<OfferStatusCheckingWorker>();
        return services;
    }
}