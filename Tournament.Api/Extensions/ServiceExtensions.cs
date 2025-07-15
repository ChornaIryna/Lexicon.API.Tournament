using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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

            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/problem+json";
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                        Title = "Authentication Required",
                        Detail = context.ErrorDescription ?? "You must be authenticated to access this resource. Please provide a valid token."
                    };
                    await context.Response.WriteAsJsonAsync(problemDetails);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/problem+json";
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                        Title = "Access Denied",
                        Detail = "You do not have the necessary permissions to perform this action."
                    };
                    await context.Response.WriteAsJsonAsync(problemDetails);
                }
            };
        });
    }
}
