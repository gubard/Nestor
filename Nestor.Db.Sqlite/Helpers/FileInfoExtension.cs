using System.Reflection;
using Gaia.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

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
        var lastMigration = GetMigrationId();

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

    private static string GetMigrationId()
    {
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x =>
            {
                var dbContextAttribute = x.GetCustomAttribute<DbContextAttribute>();

                if (dbContextAttribute is null)
                {
                    return false;
                }

                return dbContextAttribute.ContextType == typeof(SqliteNestorDbContext);
            })
            .Select(x => x.GetCustomAttribute<MigrationAttribute>())
            .Where(x => x is not null)
            .Select(x => x.ThrowIfNull().Id)
            .OrderByDescending(x => x)
            .First();
    }
}
