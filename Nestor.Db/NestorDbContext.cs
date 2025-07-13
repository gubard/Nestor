using Microsoft.EntityFrameworkCore;

namespace Nestor.Db;

public class NestorDbContext<TConfiguration> : DbContext where TConfiguration : IEntityTypeConfiguration<EventEntity>, new()
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}