using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class GameRepository(TournamentContext tournamentContext) : BaseRepository<Game>(tournamentContext), IGameRepository
{
    public async Task<IEnumerable<Game>> GetGamesByTitleAsync(string title) =>
        await tournamentContext.Games
                               .Where(g => g.Title.Contains(title, StringComparison.CurrentCultureIgnoreCase))
                               .ToListAsync();

    public async Task<IEnumerable<Game>> GetAllAsync(int tournamentDetailsId) =>
        await tournamentContext.Games
                               .Where(g => g.TournamentDetailsId == tournamentDetailsId)
                               .ToListAsync();

}
