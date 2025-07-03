using Service.Contracts.Interfaces;
using System.Reflection.Metadata;
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

        services.AddLazy<ITournamentService>();
        services.AddLazy<IGameService>();
    }
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUoW, UoW>();

        services.AddScoped<ITournamentRepository, TournamentRepository>();
        services.AddScoped<IGameRepository, GameRepository>();

        services.AddLazy<ITournamentRepository>();
        services.AddLazy<IGameRepository>();
    }
}
