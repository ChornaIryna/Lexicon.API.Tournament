using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Data.Data;

namespace Tournament.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TournamentContext>();

        await context.Database.MigrateAsync();

        if (!context.Games.Any())
        {
            // Ensure TournamentDetails exist before seeding Games
            if (!context.TournamentDetails.Any())
                await new SeedData<TournamentDetails>(context).GenerateDataAsync(3);
            await new SeedData<Games>(context).GenerateDataAsync(7);
        }
    }
}