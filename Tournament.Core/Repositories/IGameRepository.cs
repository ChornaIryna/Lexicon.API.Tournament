using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface IGameRepository : IRepository<Game>
{
    Task<IEnumerable<Game>> GetAllAsync(int tournamentDetailsId);
    Task<IEnumerable<Game>> GetGamesByTitleAsync(string title);
}
