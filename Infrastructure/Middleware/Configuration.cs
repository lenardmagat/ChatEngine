using Serilog;
using System.Security.Authentication;
using DotNetEnv;
using ChatSystem.GlobalException;
using ChatSystem.Injection;
using System.Text.Json.Serialization;
using ChatSystem.HubExcept;
using Microsoft.AspNetCore.SignalR;
namespace ChatSystem.Middleware;
class Configuration
{
    static public WebApplication webApplication()
    {
        var builder = WebApplication.CreateBuilder();
        Env.Load();
        builder.Configuration.AddEnvironmentVariables();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("Logs/chat_log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Host.UseSerilog();
         var JWTKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidCredentialException("Jwt:Key configuration is missing");
        builder.Services.AddProblemDetails();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            }
        )
            .AddJwtBearer(options =>
                {  
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],    
                        ValidAudience = builder.Configuration["Jwt:Audience"], 
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(JWTKey)) 
                    };
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ChatHub", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        // ADD THIS TO EXPOSE THE HIDDEN EXCEPTION:
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning(context.Exception, "JWT authentication failed");
                            return Task.CompletedTask;
                        }
                    };
                }
            );
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment(); // don't leak stack traces to clients in prod
            options.AddFilter<HubExceptionFilter>();
        }
        ).AddJsonProtocol(options => 
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = null; // Forces exact string casing matching
        });
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        return builder.Build();
    }
}