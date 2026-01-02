using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;
using Nestor.Db.Sqlite.CompiledModels;

namespace Nestor.Db.Sqlite;

public sealed class SqliteNestorDbContext : NestorDbContext<EventEntityTypeConfiguration>
{
    public SqliteNestorDbContext() { }

    public SqliteNestorDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseModel(SqliteNestorDbContextModel.Instance);
    }
}
