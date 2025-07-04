using System.Linq.Expressions;

namespace Tournament.Core.Repositories;
public interface IRepository<T> where T : class
{
    IQueryable<T> GetAll(bool trackChanges = false);
    IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(IQueryable<T> query, int pageNumber, int pageSize);
    Task<T?> FindByIdAsync(int id, bool trackChanges = false);
    Task<bool> AnyAsync(int id);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}
