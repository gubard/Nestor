using System;
using System.Collections.Frozen;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Nestor.Db.Services;

public interface IMigrator
{
    void Migrate(NestorDbContext dbContext);
    ConfiguredValueTaskAwaitable MigrateAsync(NestorDbContext dbContext, CancellationToken ct);
}

public sealed class Migrator : IMigrator
{
    private readonly FrozenDictionary<int, string> _migrations;

    public Migrator(FrozenDictionary<int, string> migrations)
    {
        _migrations = migrations;
    }

    public void Migrate(NestorDbContext dbContext)
    {
        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        if (databaseCreator.Exists())
        {
            var migrationCount = dbContext.Migrations.ToArray().Length;

            if (migrationCount == _migrations.Count)
            {
                return;
            }

            var migrations = _migrations
                .OrderBy(x => x.Key)
                .ToArray()
                .AsSpan()
                .Slice(migrationCount);

            foreach (var migration in migrations)
            {
                dbContext.Database.ExecuteSqlInterpolated(
                    FormattableStringFactory.Create(migration.Value)
                );

                dbContext.Migrations.Add(new() { Id = migration.Key, Sql = migration.Value });
            }

            dbContext.SaveChanges();
        }
        else
        {
            var migrations = _migrations.OrderBy(x => x.Key).ToArray();

            foreach (var migration in migrations)
            {
                dbContext.Database.ExecuteSqlInterpolated(
                    FormattableStringFactory.Create(migration.Value)
                );

                dbContext.Migrations.Add(new() { Id = migration.Key, Sql = migration.Value });
            }

            dbContext.SaveChanges();
        }
    }

    public ConfiguredValueTaskAwaitable MigrateAsync(
        NestorDbContext dbContext,
        CancellationToken ct
    )
    {
        return MigrateCore(dbContext, ct).ConfigureAwait(false);
    }

    private async ValueTask MigrateCore(NestorDbContext dbContext, CancellationToken ct)
    {
        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        if (await databaseCreator.ExistsAsync(ct))
        {
            var migrationCount = (await dbContext.Migrations.ToArrayAsync(ct)).Length;

            if (migrationCount == _migrations.Count)
            {
                return;
            }

            var migrations = _migrations
                .OrderBy(x => x.Key)
                .ToArray()
                .AsSpan()
                .Slice(migrationCount)
                .ToArray();

            foreach (var migration in migrations)
            {
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    FormattableStringFactory.Create(migration.Value),
                    ct
                );

                await dbContext.Migrations.AddAsync(
                    new() { Id = migration.Key, Sql = migration.Value },
                    ct
                );
            }

            await dbContext.SaveChangesAsync(ct);
        }
        else
        {
            var migrations = _migrations.OrderBy(x => x.Key).ToArray();

            foreach (var migration in migrations)
            {
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    FormattableStringFactory.Create(migration.Value),
                    ct
                );

                await dbContext.Migrations.AddAsync(
                    new() { Id = migration.Key, Sql = migration.Value },
                    ct
                );
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
