using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tournament.Api.Extensions;
using Tournament.Core.Entities;
using Tournament.Data.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TournamentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TournamentContext")
                         ?? throw new InvalidOperationException("Connection string 'TournamentContext' not found.")));

builder.Services.AddApiServices();
builder.Services.AddAutoMapper(typeof(TournamentMappings));
builder.Services.ConfigureServiceLayerServices();
builder.Services.ConfigureRepositories();

builder.Services.ConfigureAuthenticationWithJwtBearer(builder.Configuration);
builder.Services
    .AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TournamentContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAllOrigins");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseCors("AllowSpecificOrigins");
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await app.SeedDataAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during application startup.");
        throw;
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
