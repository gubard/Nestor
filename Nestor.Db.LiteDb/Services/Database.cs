using System.Runtime.CompilerServices;
using Nestor.Db.Services;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public interface IUltraLiteDatabase : IDatabase<UltraLiteDatabase>;

public sealed class Database : IUltraLiteDatabase
{
    public Database(UltraLiteDatabase database)
    {
        _database = database;
    }

    public ConfiguredValueTaskAwaitable ExecuteAsync(
        Func<UltraLiteDatabase, ConfiguredValueTaskAwaitable> action,
        CancellationToken ct
    )
    {
        return ExecuteCore(action, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable<T> ExecuteAsync<T>(
        Func<UltraLiteDatabase, ConfiguredValueTaskAwaitable<T>> action,
        CancellationToken ct
    )
    {
        return ExecuteCore(action, ct).ConfigureAwait(false);
    }

    private readonly UltraLiteDatabase _database;
    private readonly SemaphoreSlim _asyncSemaphore = new(1, 1);

    private async ValueTask ExecuteCore(
        Func<UltraLiteDatabase, ConfiguredValueTaskAwaitable> action,
        CancellationToken ct
    )
    {
        await _asyncSemaphore.WaitAsync(ct);

        try
        {
            action(_database);
        }
        finally
        {
            _asyncSemaphore.Release();
        }
    }

    private async ValueTask<T> ExecuteCore<T>(
        Func<UltraLiteDatabase, ConfiguredValueTaskAwaitable<T>> action,
        CancellationToken ct
    )
    {
        await _asyncSemaphore.WaitAsync(ct);

        try
        {
            return await action(_database);
        }
        finally
        {
            _asyncSemaphore.Release();
        }
    }
}
