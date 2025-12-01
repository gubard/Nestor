using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;

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