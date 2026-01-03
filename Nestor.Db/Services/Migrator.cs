using System;
using System.Collections.Frozen;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface IMigrator
{
    void Migrate(DbContext dbContext);
    ValueTask MigrateAsync(DbContext dbContext, CancellationToken ct);
}

public sealed class Migrator : IMigrator
{
    private readonly FrozenDictionary<long, string> _migrations;

    public Migrator(FrozenDictionary<long, string> migrations)
    {
        _migrations = migrations;
    }

    public void Migrate(DbContext dbContext)
    {
        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        if (databaseCreator.Exists())
        {
            var migrationCount = dbContext.Set<MigrationEntity>().ToArray().Length;

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
                dbContext.Database.ExecuteSqlRaw(migration.Value);

                dbContext
                    .Set<MigrationEntity>()
                    .Add(new() { Id = migration.Key, Sql = migration.Value });
            }

            dbContext.SaveChanges();
        }
        else
        {
            var migrations = _migrations.OrderBy(x => x.Key).ToArray();

            foreach (var migration in migrations)
            {
                dbContext.Database.ExecuteSqlRaw(migration.Value);
                dbContext
                    .Set<MigrationEntity>()
                    .Add(new() { Id = migration.Key, Sql = migration.Value });
            }

            dbContext.SaveChanges();
        }
    }

    public async ValueTask MigrateAsync(DbContext dbContext, CancellationToken ct)
    {
        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        if (await databaseCreator.ExistsAsync(ct))
        {
            var migrationCount = (await dbContext.Set<MigrationEntity>().ToArrayAsync(ct)).Length;

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
                await dbContext.Database.ExecuteSqlRawAsync(migration.Value, ct);

                dbContext
                    .Set<MigrationEntity>()
                    .Add(new() { Id = migration.Key, Sql = migration.Value });
            }

            await dbContext.SaveChangesAsync(ct);
        }
        else
        {
            var migrations = _migrations.OrderBy(x => x.Key).ToArray();

            foreach (var migration in migrations)
            {
                await dbContext.Database.ExecuteSqlRawAsync(migration.Value, ct);

                dbContext
                    .Set<MigrationEntity>()
                    .Add(new() { Id = migration.Key, Sql = migration.Value });
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
