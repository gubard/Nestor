using Gaia.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Nestor.Db.Sqlite.Helpers;

public static class FileInfoExtension
{
    public static SqliteNestorDbContext InitDbContext(this FileInfo file)
    {
        var options = new DbContextOptionsBuilder<SqliteNestorDbContext>()
            .UseSqlite(
                $"Data Source={file}",
                x => x.MigrationsAssembly(typeof(SqliteNestorDbContext).Assembly)
            )
            .Options;

        var context = new SqliteNestorDbContext(options);
        var migrationFile = file.FileInSameDir($"{file.GetFileNameWithoutExtension()}.migration");
        var lastMigration = context.Database.GetMigrations().Last();

        if (!file.Exists)
        {
            if (file.Directory is { Exists: false })
            {
                file.Directory.Create();
            }

            context.Database.Migrate();
            migrationFile.WriteAllText(lastMigration);
        }
        else if (lastMigration != migrationFile.ReadAllText())
        {
            context.Database.Migrate();
            migrationFile.WriteAllText(lastMigration);
        }

        return context;
    }
}
