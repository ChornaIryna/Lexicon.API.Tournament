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

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(IQueryable<T> query, int pageNumber, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual IQueryable<T> GetAll(bool trackChanges = false) =>
        trackChanges
        ? DbSet
        : DbSet.AsNoTracking();

    public virtual async Task<T?> FindByIdAsync(int id, bool trackChanges = false)
    {
        var entity = await DbSet.FindAsync(id);
        if (!trackChanges && entity != null)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual void Add(T entity) => Context.Add(entity);

    public virtual void Remove(T entity) => Context.Remove(entity);

    public virtual void Update(T entity) => Context.Update(entity);

    public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false) =>
        trackChanges
        ? DbSet.Where(expression)
        : DbSet.Where(expression).AsNoTracking();
}
