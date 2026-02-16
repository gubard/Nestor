using System.Collections.Frozen;
using System.Collections.Generic;
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
    public Migrator(FrozenDictionary<int, string> migrations)
    {
        _migrations = migrations;
    }

    public void Migrate(IDbConnectionFactory factory)
    {
        if (factory.IsCanConnect(TestQuery))
        {
            using var session = factory.CreateSession();
            var ids = GetUnappliedMigrations(session);

            foreach (var id in ids)
            {
                var sql = _migrations[id];
                session.ExecuteNonQuery(sql);

                session.ExecuteNonQuery(
                    new MigrationEntity[]
                    {
                        new() { Id = id, Sql = sql },
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

    private static readonly SqlQuery MigrationIdsQuery = "SELECT Id FROM Migrations";
    private static readonly SqlQuery TestQuery = "SELECT * FROM Migrations;";

    private readonly FrozenDictionary<int, string> _migrations;

    private async ValueTask<int[]> GetUnappliedMigrationsAsync(
        DbSession session,
        CancellationToken ct
    )
    {
        await using var reader = await session.ExecuteReaderAsync(MigrationIdsQuery, ct);

        if (!reader.HasRows)
        {
            return _migrations.Select(x => x.Key).Order().ToArray();
        }

        var ids = new List<int>();

        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetFieldValue<int>(0));
        }

        return _migrations.Select(x => x.Key).Except(ids).Order().ToArray();
    }

    private int[] GetUnappliedMigrations(DbSession session)
    {
        using var reader = session.ExecuteReader(MigrationIdsQuery);

        if (!reader.HasRows)
        {
            return _migrations.Select(x => x.Key).Order().ToArray();
        }

        var ids = new List<int>();

        while (reader.Read())
        {
            ids.Add(reader.GetFieldValue<int>(0));
        }

        return _migrations.Select(x => x.Key).Except(ids).Order().ToArray();
    }

    private async ValueTask MigrateCore(IDbConnectionFactory factory, CancellationToken ct)
    {
        if (await factory.IsCanConnectAsync(TestQuery, ct))
        {
            await using var session = factory.CreateSession();
            var ids = await GetUnappliedMigrationsAsync(session, ct);

            foreach (var id in ids)
            {
                var sql = _migrations[id];
                await session.ExecuteNonQueryAsync(sql, ct);

                await session.ExecuteNonQueryAsync(
                    new MigrationEntity[]
                    {
                        new() { Id = id, Sql = sql },
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
