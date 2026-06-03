using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nestor.Db.Services;

namespace Nestor.Db.Helpers;

public static class DatabaseExtension
{
    public static ConfiguredValueTaskAwaitable ExecuteAsync<TDb>(
        this IDatabase<TDb> database,
        Func<TDb, ValueTask> action,
        CancellationToken ct
    )
    {
        return database.ExecuteAsync(db => action.Invoke(db).ConfigureAwait(false), ct);
    }

    public static ConfiguredValueTaskAwaitable<T> ExecuteAsync<TDb, T>(
        this IDatabase<TDb> database,
        Func<TDb, ValueTask<T>> action,
        CancellationToken ct
    )
    {
        return database.ExecuteAsync(db => action.Invoke(db).ConfigureAwait(false), ct);
    }
}
