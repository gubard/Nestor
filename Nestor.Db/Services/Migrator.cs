using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nestor.Db.Helpers;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface IMigrator
{
    ConfiguredValueTaskAwaitable MigrateAsync(IAdoDatabase dbContext, CancellationToken ct);
}

public sealed class Migrator : IMigrator
{
    public Migrator(FrozenDictionary<int, SqlQuery> migrations)
    {
        _migrations = migrations;
    }

    public ConfiguredValueTaskAwaitable MigrateAsync(IAdoDatabase database, CancellationToken ct)
    {
        return MigrateCore(database, ct).ConfigureAwait(false);
    }

    private static readonly SqlQuery MigrationIdsQuery = "SELECT Id FROM Migrations";

    private readonly FrozenDictionary<int, SqlQuery> _migrations;

    private async ValueTask MigrateCore(IAdoDatabase database, CancellationToken ct)
    {
        try
        {
            await database.ExecuteAsync(
                async command =>
                {
                    var ids = await GetUnappliedMigrationsAsync(command, ct).ConfigureAwait(false);

                    foreach (var id in ids)
                    {
                        var sql = _migrations[id];
                        await command.ExecuteNonQueryAsync(sql, ct);

                        await command.ExecuteNonQueryAsync(
                            new MigrationEntity[]
                            {
                                new() { Id = id, Sql = sql.Sql },
                            }.CreateInsertQuery(),
                            ct
                        );
                    }
                },
                ct
            );
        }
        catch
        {
            await database.ExecuteAsync(
                async command =>
                {
                    foreach (var migration in _migrations)
                    {
                        await command.ExecuteNonQueryAsync(migration.Value, ct);

                        await command.ExecuteNonQueryAsync(
                            new MigrationEntity[]
                            {
                                new() { Id = migration.Key, Sql = migration.Value.Sql },
                            }.CreateInsertQuery(),
                            ct
                        );
                    }
                },
                ct
            );
        }
    }

    private async ValueTask<int[]> GetUnappliedMigrationsAsync(
        DbCommand command,
        CancellationToken ct
    )
    {
        await using var reader = await command.ExecuteReaderAsync(MigrationIdsQuery, ct);

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
}
