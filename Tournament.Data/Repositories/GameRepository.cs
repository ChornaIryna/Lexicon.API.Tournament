using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class GameRepository(TournamentContext tournamentContext) : BaseRepository<Game>(tournamentContext), IGameRepository
{
    public async Task<IEnumerable<Game>> GetGamesByTitleAsync(int id, string? title)
    {
        return await GetByConditionAsync(g =>
            g.TournamentDetailsId == id &&
            (string.IsNullOrEmpty(title) ||
             g.Title != null &&
             EF.Functions.Like(g.Title, $"%{title}%")));
    }
}
