using System.Threading;
using System.Threading.Tasks;
using DAL.SourceGen.Sample.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.SourceGen.Sample;

public interface IAppDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}