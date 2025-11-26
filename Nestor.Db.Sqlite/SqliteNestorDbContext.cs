using Microsoft.EntityFrameworkCore;

namespace Nestor.Db.Sqlite;

public sealed class SqliteNestorDbContext : NestorDbContext<EventEntityTypeConfiguration>
{
    public SqliteNestorDbContext()
    {
    }

    public SqliteNestorDbContext(DbContextOptions options) : base(options)
    {
    }
}