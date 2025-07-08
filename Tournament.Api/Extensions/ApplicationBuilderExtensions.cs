using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Data.Data;

namespace Tournament.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var context = serviceProvider.GetRequiredService<TournamentContext>();
        var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        if (environment.IsDevelopment())
        {
            if (!context.Games.Any())
            {
                // Ensure TournamentDetails exist before seeding Game
                if (!context.TournamentDetails.Any())
                    await new SeedData<TournamentDetails>(context).GenerateDataAsync(3);
                await new SeedData<Game>(context).GenerateDataAsync(7);
            }

            if (!context.Users.Any())
                await new SeedData<ApplicationUser>(context).GenerateAdminUserAsync(configuration, userManager);
        }

    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}