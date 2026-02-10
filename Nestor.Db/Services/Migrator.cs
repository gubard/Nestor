using System;
using System.Collections.Frozen;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nestor.Db.Helpers;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface IMigrator
{
    void Migrate(IDbConnectionFactory dbContext);
    ConfiguredValueTaskAwaitable MigrateAsync(IDbConnectionFactory dbContext, CancellationToken ct);
}

public sealed class Migrator : IMigrator
{
    private static readonly SqlQuery TestQuery = "SELECT * FROM Migrations;";
    private static readonly SqlQuery MigrationsCountQuery = "SELECT COUNT(*) FROM Migrations;";

    private readonly FrozenDictionary<int, string> _migrations;

    public Migrator(FrozenDictionary<int, string> migrations)
    {
        _migrations = migrations;
    }

    public void Migrate(IDbConnectionFactory factory)
    {
        if (factory.IsCanConnect(TestQuery))
        {
            using var session = factory.CreateSession();
            var migrationCount = session.ExecuteScalarInt32(MigrationsCountQuery);

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
                session.ExecuteNonQuery(migration.Value);

                session.ExecuteNonQuery(
                    new MigrationEntity[]
                    {
                        new() { Id = migration.Key, Sql = migration.Value },
                    }.CreateInsertQuery()
                );
            }

            session.Commit();
        }
        else
        {
            using var session = factory.CreateSession();
            var migrations = _migrations.OrderBy(x => x.Key).ToArray();

            foreach (var migration in migrations)
            {
                session.ExecuteNonQuery(migration.Value);

                session.ExecuteNonQuery(
                    new MigrationEntity[]
                    {
                        new() { Id = migration.Key, Sql = migration.Value },
                    }.CreateInsertQuery()
                );
            }

            session.Commit();
        }
    }

    public ConfiguredValueTaskAwaitable MigrateAsync(
        IDbConnectionFactory factory,
        CancellationToken ct
    )
    {
        return MigrateCore(factory, ct).ConfigureAwait(false);
    }

    private async ValueTask MigrateCore(IDbConnectionFactory factory, CancellationToken ct)
    {
        if (await factory.IsCanConnectAsync(TestQuery, ct))
        {
            await using var session = factory.CreateSession();
            var migrationCount = await session.ExecuteScalarInt32Async(MigrationsCountQuery, ct);

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
                await session.ExecuteNonQueryAsync(migration.Value, ct);

                await session.ExecuteNonQueryAsync(
                    new MigrationEntity[]
                    {
                        new() { Id = migration.Key, Sql = migration.Value },
                    }.CreateInsertQuery(),
                    ct
                );
            }

            await session.CommitAsync(ct);
        }
        else
        {
            await using var session = factory.CreateSession();
            var migrations = _migrations.OrderBy(x => x.Key).ToArray();

            foreach (var migration in migrations)
            {
                await session.ExecuteNonQueryAsync(migration.Value, ct);

                await session.ExecuteNonQueryAsync(
                    new MigrationEntity[]
                    {
                        new() { Id = migration.Key, Sql = migration.Value },
                    }.CreateInsertQuery(),
                    ct
                );
            }

            await session.CommitAsync(ct);
        }
    }
}
