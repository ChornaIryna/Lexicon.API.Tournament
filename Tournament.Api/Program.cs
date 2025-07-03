using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Tournament.Api.Extensions;
using Tournament.Data.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TournamentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TournamentContext") ?? throw new InvalidOperationException("Connection string 'TournamentContext' not found.")));

builder.Services.AddApiServices();
builder.Services.AddAutoMapper(typeof(TournamentMappings));
builder.Services.ConfigureServiceLayerServices();
builder.Services.ConfigureRepositories();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.SeedDataAsync();
    app.UseCors("AllowAllOrigins");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseCors("AllowSpecificOrigins");
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
