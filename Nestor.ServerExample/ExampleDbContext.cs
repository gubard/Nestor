using Microsoft.EntityFrameworkCore;
using Nestor.Db;

namespace Nestor.ServerExample;

public class ExampleDbContext : DbContext
{
    public ExampleDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<EventEntity> Events { get; set; }
}
