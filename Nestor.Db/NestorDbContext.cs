using Microsoft.EntityFrameworkCore;

namespace Nestor.Db;

public abstract class NestorDbContext<TConfiguration> : DbContext
    where TConfiguration : IEntityTypeConfiguration<EventEntity>, new()
{
    protected NestorDbContext()
    {
    }

    protected NestorDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}