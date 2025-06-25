using Microsoft.EntityFrameworkCore;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public abstract class BaseRepository<T>(TournamentContext context) : IRepository<T> where T : class
{
    public virtual async Task<bool> AnyAsync(int id) =>
        await context.Set<T>().AnyAsync(e => EF.Property<int>(e, "Id") == id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await context.Set<T>().ToListAsync();
    public virtual async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);
    public virtual void Add(T entity) => context.Add(entity);
    public virtual void Remove(T entity) => context.Remove(entity);
    public virtual void Update(T entity) => context.Update(entity);
}
