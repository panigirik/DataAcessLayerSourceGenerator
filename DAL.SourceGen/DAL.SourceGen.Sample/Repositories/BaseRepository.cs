using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.SourceGen.Sample.Repositories;

using Microsoft.EntityFrameworkCore;

public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly IAppDbContext _db;

    protected BaseRepository(IAppDbContext db)
    {
        _db = db;
    }

    public IQueryable<TEntity> Query => _db.Set<TEntity>();

    public virtual TEntity? GetById(Guid id)
    {
        return Query.FirstOrDefault(e => EF.Property<Guid>(e, "Id") == id);
    }

    public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return Query.FirstOrDefaultAsync(
            e => EF.Property<Guid>(e, "Id") == id,
            ct);
    }

    public virtual void Add(TEntity entity)
    {
        _db.Set<TEntity>().Add(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        _db.Set<TEntity>().Remove(entity);
    }
}
