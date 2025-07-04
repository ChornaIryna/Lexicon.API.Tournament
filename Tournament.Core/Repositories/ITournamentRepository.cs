using System.Linq.Expressions;
using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface ITournamentRepository : IRepository<TournamentDetails>
{
    IQueryable<TournamentDetails> GetFiltered(Expression<Func<TournamentDetails, bool>>? filter = null, bool includeGames = false, bool trackChanges = false);
}
