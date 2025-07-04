using System.Linq.Expressions;
using Tournament.Core.Entities;

namespace Tournament.Core.Repositories;
public interface IGameRepository : IRepository<Game>
{
    IQueryable<Game> GetFiltered(Expression<Func<Game, bool>>? filter = null, bool trackChanges = false);
}
