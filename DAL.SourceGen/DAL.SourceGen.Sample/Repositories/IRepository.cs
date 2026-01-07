using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.SourceGen.Sample.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query { get; }
    TEntity? GetById(Guid id);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(TEntity entity);
    void Remove(TEntity entity);
}
