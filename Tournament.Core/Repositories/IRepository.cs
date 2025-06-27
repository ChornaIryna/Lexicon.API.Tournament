using System.Linq.Expressions;

namespace Tournament.Core.Repositories;
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(bool trackChanges = false);
    Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<T?> FindByIdAsync(int id, bool trackChanges = false);
    Task<bool> AnyAsync(int id);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}
