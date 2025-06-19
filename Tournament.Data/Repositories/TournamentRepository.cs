using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class TournamentRepository(TournamentContext context) : ITournamentRepository
{
    public void Add(TournamentDetails tournamentDetails) => context.Add(tournamentDetails);
    public async Task<bool> AnyAsync(int id) => await context.TournamentDetails.AnyAsync(t => t.Id == id);
    public async Task<IEnumerable<TournamentDetails>> GetAllTournamentsAsync() =>
        await context.TournamentDetails.Include(t => t.Games).ToListAsync();
    public async Task<TournamentDetails?> GetTournamentByIdAsync(int id) =>
        await context.TournamentDetails.Include(t => t.Games).FirstOrDefaultAsync(t => t.Id == id);
    public void Remove(TournamentDetails tournamentDetails) => context.Remove(tournamentDetails);
    public void Update(TournamentDetails tournamentDetails) => context.Update(tournamentDetails);
}
