namespace Tournament.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(option => option.ReturnHttpNotAcceptable = true)
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();
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
}
