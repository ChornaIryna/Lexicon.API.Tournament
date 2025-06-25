using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface ITournamentRepository : IRepository<TournamentDetails>
{
    Task<IEnumerable<TournamentDetails>> GetAllAsync(bool includeGames = false);
}
