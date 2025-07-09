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
        var query = filter == null
            ? GetAll(trackChanges)
            : GetByCondition(filter, trackChanges);
        if (includeGames)
            query = query.Include(t => t.Games);
        return query;
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
    public async Task<int> CountGamesAsync(int tournamentId) =>
        await Context.Games.CountAsync(g => g.TournamentDetailsId == tournamentId);
}
