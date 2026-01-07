using DAL.SourceGen.Sample.Configurations;
using DAL.SourceGen.Sample.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.SourceGen.Sample;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
}