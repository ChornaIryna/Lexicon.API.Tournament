using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Tournament.Core.Entities;
namespace Tournament.Data.Data;

public class SeedData<T>(TournamentContext context) where T : class
{
    public async Task GenerateDataAsync(int count)
    {
        var entities = new List<T>();
        for (int i = 0; i < count; i++)
        {
            var entity = await CreateEntityAsync(i);
            if (entity != null)
                entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            context.Set<T>().AddRange(entities);
            await context.SaveChangesAsync();
        }
    }

    public async Task GenerateUsersAsync(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        var password = configuration["DefaultAdminPassword"];
        List<ApplicationUser> users =
        [
            new()
            {
                Name = "Admin",
                Age = 20,
                Position = "Admin in Development",
                Email = "admin@test.email",
                UserName = $"admin@test.email",
            },
            new()
            {
                Name = "Regular User",
                Age = 50,
                Email = "user@test.email",
                UserName = $"user@test.email"
            }
        ];
        foreach (ApplicationUser user in users)
            await CreateUserAsync(userManager, password, user);
    }

    private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager, string? password, ApplicationUser user)
    {
        var result = await userManager.CreateAsync(user, password!);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(user.Position))
                await userManager.AddToRoleAsync(user, "Admin");
            else
                await userManager.AddToRoleAsync(user, "User");
        }

        else
            Console.WriteLine($"Error occurred when creating user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    private async Task<T?> CreateEntityAsync(int index)
    {
        if (typeof(T) == typeof(TournamentDetails))
        {
            return new TournamentDetails
            {
                Title = $"Tournament {index + 1}",
                StartDate = DateTime.UtcNow.AddDays(index)
            } as T;
        }

        if (typeof(T) == typeof(Game))
        {
            // Ensure there is at least one TournamentDetails to reference
            var tournamentIds = context.TournamentDetails.Select(td => td.Id).ToList();
            if (tournamentIds.Count == 0)
            {
                var tournament = new TournamentDetails
                {
                    Title = "First Generated Tournament",
                    StartDate = DateTime.UtcNow
                };
                context.TournamentDetails.Add(tournament);
                await context.SaveChangesAsync();
                tournamentIds.Add(tournament.Id);
            }

            var randomTournamentId = tournamentIds[Random.Shared.Next(tournamentIds.Count)];

            return new Game
            {
                Title = $"Game {index + 1}",
                Time = DateTime.UtcNow.AddDays(randomTournamentId - 1).AddHours(index),
                TournamentDetailsId = randomTournamentId
            } as T;
        }

        return null;
    }
}