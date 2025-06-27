using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class TournamentRepository(TournamentContext tournamentContext) : BaseRepository<TournamentDetails>(tournamentContext), ITournamentRepository
{
    public async Task<IEnumerable<TournamentDetails>> GetAllAsync(bool includeGames, bool trackChanges = false)
    {
        var query = Context.TournamentDetails;

        IQueryable<TournamentDetails> tournaments = trackChanges
                                                    ? query
                                                    : query.AsNoTracking();

        if (includeGames)
            tournaments = tournaments.Include(t => t.Games);

        return await tournaments.ToListAsync();
    }

    public override async Task<TournamentDetails?> FindByIdAsync(int id, bool trackChanges = false) =>
        trackChanges
            ? await Context.TournamentDetails
                    .Include(t => t.Games)
                    .FirstOrDefaultAsync(t => t.Id == id)
            : await Context.TournamentDetails
                    .AsNoTracking()
                    .Include(t => t.Games)
                    .FirstOrDefaultAsync(t => t.Id == id);
}
