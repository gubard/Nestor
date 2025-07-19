using Microsoft.EntityFrameworkCore;

namespace Nestor.Db;

public class NestorDbContext<TConfiguration> : DbContext
    where TConfiguration : IEntityTypeConfiguration<EventEntity>, new()
{
    public NestorDbContext()
    {
    }

    public NestorDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}