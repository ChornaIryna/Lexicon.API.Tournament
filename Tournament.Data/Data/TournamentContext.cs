using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;

namespace Tournament.Data.Data;

public class TournamentContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public TournamentContext(DbContextOptions<TournamentContext> options)
        : base(options)
    {
    }

    public DbSet<TournamentDetails> TournamentDetails { get; set; } = default!;
    public DbSet<Game> Games { get; set; } = default!;
}
