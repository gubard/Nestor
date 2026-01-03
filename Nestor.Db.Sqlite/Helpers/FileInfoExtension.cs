using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;

namespace Nestor.Db.Sqlite.Helpers;

public static class FileInfoExtension
{
    public static SqliteNestorDbContext InitDbContext(this FileInfo file, IMigrator migrator)
    {
        var options = new DbContextOptionsBuilder<SqliteNestorDbContext>()
            .UseSqlite($"Data Source={file}")
            .Options;

        var context = new SqliteNestorDbContext(options);
        migrator.Migrate(context);

        return context;
    }
}
