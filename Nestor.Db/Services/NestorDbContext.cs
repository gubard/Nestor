using Microsoft.EntityFrameworkCore;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public abstract class NestorDbContext : DbContext
{
    protected NestorDbContext() { }

    protected NestorDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationEntityTypeConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
