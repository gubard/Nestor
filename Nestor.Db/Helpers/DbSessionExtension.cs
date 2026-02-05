using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Nestor.Db.Helpers;

public static class DbSessionExtension
{
    public static DbParameter[] ToDbParameters<T>(
        this DbSession dbSession,
        T[] items,
        string parameterName
    )
    {
        var result = new DbParameter[items.Length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = dbSession.CreateParameter($"@{parameterName}{i}", items[i]);
        }

        return result;
    }

    public static ConfiguredValueTaskAwaitable<Guid[]> GetGuidAsync(
        this DbSession session,
        SqlQuery query,
        CancellationToken ct
    )
    {
        return GetGuidCore(session, query, ct).ConfigureAwait(false);
    }

    private static async ValueTask<Guid[]> GetGuidCore(
        this DbSession session,
        SqlQuery query,
        CancellationToken ct
    )
    {
        var ids = new List<Guid>();
        await using var reader = await session.ExecuteReaderAsync(query, ct);

        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids.ToArray();
    }
}
