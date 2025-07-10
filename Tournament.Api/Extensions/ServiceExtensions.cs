using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts.Interfaces;
using System.Reflection.Metadata;
using Tournament.Core.Configuration;
using Tournament.Core.Repositories;
using Tournament.Data.Repositories;
using Tournament.Services.Implementations;

namespace Tournament.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(option => option.ReturnHttpNotAcceptable = true)
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters()
                .AddApplicationPart(typeof(AssemblyReference).Assembly);
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder => builder.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader());
            options.AddPolicy("AllowSpecificOrigins",
                builder => builder.WithOrigins("https://example.com", "https://another-example.com")
                                  .AllowAnyMethod()
                                  .AllowAnyHeader());
        });
        return services;
    }

    public static void ConfigureServiceLayerServices(this IServiceCollection services)
    {
        services.AddScoped<IServiceManager, ServiceManager>();

        services.AddScoped<ITournamentService, TournamentService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddLazy<ITournamentService>();
        services.AddLazy<IGameService>();
        services.AddLazy<IAuthService>();
    }
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUoW, UoW>();

        services.AddScoped<ITournamentRepository, TournamentRepository>();
        services.AddScoped<IGameRepository, GameRepository>();

        services.AddLazy<ITournamentRepository>();
        services.AddLazy<IGameRepository>();
    }

    public static void ConfigureAuthenticationWithJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwTSettings");
        ArgumentNullException.ThrowIfNull(nameof(jwtSettings));

        var key = jwtSettings["Key"];
        ArgumentNullException.ThrowIfNull(nameof(key));

        var jwtConfiguration = new JwtConfiguration();
        jwtSettings.Bind(jwtConfiguration);

        services.Configure<JwtConfiguration>(options =>
        {
            options.Issuer = jwtConfiguration.Issuer;
            options.Audience = jwtConfiguration.Audience;
            options.Key = jwtConfiguration.Key;
            options.ExpirationsMinutes = jwtConfiguration.ExpirationsMinutes;
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key!))
            };
        });
    }
}
