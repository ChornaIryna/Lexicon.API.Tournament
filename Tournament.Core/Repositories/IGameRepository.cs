using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllTournamentGamesAsync(int tournamentDetailsId);
    Task<IEnumerable<Game>> GetAllGamesAsync();
    Task<Game?> GetGameByIdAsync(int id);
    Task<bool> AnyAsync();
    Task<bool> GameExists(int id);
    void Add(Game game);
    void Update(Game game);
    void Remove(Game game);
}
