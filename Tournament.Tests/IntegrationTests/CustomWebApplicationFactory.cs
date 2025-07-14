using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tournament.Data.Data;

namespace Tournament.Tests.IntegrationTests;
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TournamentContext>));
            services.RemoveAll(typeof(TournamentContext));

            services.AddDbContext<TournamentContext>(options =>
            {
                options.UseInMemoryDatabase("TestTournamentDb");
            });

            // Ensure the database is created and seeded for each test run
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TournamentContext>();

                // Ensure the database is clean before each test
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

            }
        });
    }

    /// <summary>
    /// Generates a valid JWT token for a test user.
    /// </summary>
    public string GenerateJwtToken(string userId, string userName, string? role = null)
    {
        var jwtSettingsKey = "super-secret-key-that-is-at-least-32-characters-long";

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userId),
            new (ClaimTypes.Name, userName),
            new (JwtRegisteredClaimNames.Sub, userId)
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettingsKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TournamentIssuer",
            audience: "https://localhost:7282",
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
