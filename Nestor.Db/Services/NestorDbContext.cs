using Microsoft.EntityFrameworkCore;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public abstract class NestorDbContext : DbContext
{
    protected NestorDbContext() { }

    protected NestorDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<EventEntity> Events { get; set; }
    public DbSet<EventEntity> Migrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new EventEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationEntityTypeConfiguration());
    }
}
