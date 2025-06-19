using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class GameRepository(TournamentContext context) : IGameRepository
{
    public void Add(Game game) => context.Games.Add(game);
    public async Task<bool> AnyAsync(int id) => await context.Games.AnyAsync(g => g.Id == id);
    public async Task<IEnumerable<Game>> GetAllGamesAsync() => await context.Games.ToListAsync();
    public async Task<IEnumerable<Game>> GetAllTournamentGamesAsync(int tournamentDetailsId) =>
        await context.Games.Where(g => g.TournamentDetailsId == tournamentDetailsId).ToListAsync();
    public async Task<Game?> GetGameByIdAsync(int id) => await context.Games.FindAsync(id);
    public void Remove(Game game) => context.Remove(game);
    public void Update(Game game) => context.Update(game);
}
