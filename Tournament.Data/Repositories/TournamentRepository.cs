using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class TournamentRepository(TournamentContext tournamentContext) : BaseRepository<TournamentDetails>(tournamentContext), ITournamentRepository
{
    public IQueryable<TournamentDetails> GetFiltered(Expression<Func<TournamentDetails, bool>>? filter = null, bool includeGames = false, bool trackChanges = false)
    {
        var query = GetAll();

        IQueryable<TournamentDetails> tournaments = trackChanges
                                                    ? query
                                                    : query.AsNoTracking();
        if (filter != null)
            tournaments = tournaments.Where(filter);

        if (includeGames)
            tournaments = tournaments.Include(t => t.Games);
        return tournaments;
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
