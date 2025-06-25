using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class TournamentRepository(TournamentContext tournamentContext) : BaseRepository<TournamentDetails>(tournamentContext), ITournamentRepository
{
    public async Task<IEnumerable<TournamentDetails>> GetAllAsync(bool includeGames) =>
       includeGames ? await tournamentContext.TournamentDetails.Include(t => t.Games).ToListAsync()
                     : await tournamentContext.TournamentDetails.ToListAsync();
    public override async Task<TournamentDetails?> GetByIdAsync(int id) =>
        await tournamentContext.TournamentDetails.Include(t => t.Games).FirstOrDefaultAsync(t => t.Id == id);
}
