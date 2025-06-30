using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface IGameRepository : IRepository<Game>
{
    Task<IEnumerable<Game>> GetGamesByTitleAsync(int id, string title);
    Task<IEnumerable<Game>> SearchGamesByTitleAsync(int id, string? title);
}
