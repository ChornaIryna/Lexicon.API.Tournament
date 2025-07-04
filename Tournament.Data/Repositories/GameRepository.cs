using System.Linq.Expressions;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public class GameRepository(TournamentContext tournamentContext) : BaseRepository<Game>(tournamentContext), IGameRepository
{
    public IQueryable<Game> GetFiltered(Expression<Func<Game, bool>>? filter = null, bool trackChanges = false)
    {
        var query = filter == null
            ? GetAll(trackChanges)
            : GetByCondition(filter, trackChanges);
        return query;
    }
}
