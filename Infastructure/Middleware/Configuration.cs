using Serilog;
using System.Security.Authentication;
using DotNetEnv;
using ChatSystem.GlobalException;
using ChatSystem.Injection;
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
                        .CreateLogger();
        builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
        builder.Host.UseSerilog();
         var JWTKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidCredentialException("");
        builder.Services.AddAuthentication(options =>
            {

                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
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
            });
        builder.Services.AddApplicationServices(builder.Configuration);
        return builder.Build();
    }
}