using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface INestorDbContext
{
    DbSet<EventEntity> Events { get; }
    DbSet<MigrationEntity> Migrations { get; }

    void AddRange(params object[] entities);
    Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken);
}

public abstract class NestorDbContext : DbContext, INestorDbContext
{
    protected NestorDbContext() { }

    protected NestorDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<EventEntity> Events { get; set; }
    public DbSet<MigrationEntity> Migrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EventEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationEntityTypeConfiguration());
    }
}
