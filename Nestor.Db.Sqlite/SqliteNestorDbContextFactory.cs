using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nestor.Db.Sqlite;

public class SqliteNestorDbContextFactory : IDesignTimeDbContextFactory<SqliteNestorDbContext>
{
    public SqliteNestorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteNestorDbContext>();
        optionsBuilder.UseSqlite("");

        return new(optionsBuilder.Options);
    }
}