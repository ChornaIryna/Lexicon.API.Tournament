using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Tournament.Core.Repositories;
using Tournament.Data.Data;

namespace Tournament.Data.Repositories;
public abstract class BaseRepository<T>(TournamentContext context) : IRepository<T> where T : class
{
    protected DbSet<T> DbSet { get; } = context.Set<T>();
    protected TournamentContext Context { get; } = context;

    public virtual async Task<bool> AnyAsync(int id) =>
        await DbSet.AnyAsync(e => EF.Property<int>(e, "Id") == id);

    public virtual async Task<IEnumerable<T>> GetAllAsync(bool trackChanges = false) =>
        trackChanges
        ? await DbSet.ToListAsync()
        : await DbSet.AsNoTracking().ToListAsync();

    public virtual async Task<T?> FindByIdAsync(int id, bool trackChanges = false)
    {
        var entity = await DbSet.FindAsync(id);
        if (!trackChanges && entity != null)
        {
            context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual void Add(T entity) => context.Add(entity);

    public virtual void Remove(T entity) => context.Remove(entity);

    public virtual void Update(T entity) => context.Update(entity);

    public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
        trackChanges
        ? DbSet.Where(expression)
        : DbSet.Where(expression).AsNoTracking();
}
