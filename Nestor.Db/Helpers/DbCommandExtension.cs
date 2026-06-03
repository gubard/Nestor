using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nestor.Db.Models;

namespace Nestor.Db.Helpers;

public static class DbCommandExtension
{
    public static ConfiguredValueTaskAwaitable<DbDataReader> ExecuteReaderAsync(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        return ExecuteReaderCore(command, query, ct).ConfigureAwait(false);
    }

    private static async ValueTask<DbDataReader> ExecuteReaderCore(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        command.SetSqlQuery(query);

        return await command.ExecuteReaderAsync(ct);
    }

    private static async ValueTask<Guid[]> GetGuidCore(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        var ids = new List<Guid>();
        command.SetSqlQuery(query);
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids.ToArray();
    }

    public static ConfiguredValueTaskAwaitable<Guid[]> GetGuidAsync(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        return GetGuidCore(command, query, ct).ConfigureAwait(false);
    }

    public static ConfiguredValueTaskAwaitable<int> ExecuteNonQueryAsync(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        return command.ExecuteNonQueryCore(query, ct).ConfigureAwait(false);
    }

    private static async ValueTask<int> ExecuteNonQueryCore(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        command.SetSqlQuery(query);

        return await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static void SetSqlQuery(this DbCommand command, SqlQuery query)
    {
        command.CommandText = query.Sql;
        command.Parameters.Clear();
        command.Parameters.AddRange(query.CreateParameters(command));
    }

    public static ConfiguredValueTaskAwaitable<object?> ExecuteScalarAsync(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        return command.ExecuteScalarCore(query, ct).ConfigureAwait(false);
    }

    private static async ValueTask<object?> ExecuteScalarCore(
        this DbCommand command,
        SqlQuery query,
        CancellationToken ct
    )
    {
        command.SetSqlQuery(query);

        return await command.ExecuteScalarAsync(ct).ConfigureAwait(false);
    }
}
