using System.Runtime.CompilerServices;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public sealed class Database : IDatabase
{
    public Database(UltraLiteDatabase database)
    {
        _database = database;
    }

    public ConfiguredValueTaskAwaitable ExecuteAsync(
        Action<UltraLiteDatabase> action,
        CancellationToken ct
    )
    {
        return ExecuteCore(action, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable<T> ExecuteAsync<T>(
        Func<UltraLiteDatabase, T> action,
        CancellationToken ct
    )
    {
        return ExecuteCore(action, ct).ConfigureAwait(false);
    }

    private readonly UltraLiteDatabase _database;
    private readonly SemaphoreSlim _asyncSemaphore = new(1, 1);

    private async ValueTask ExecuteCore(Action<UltraLiteDatabase> action, CancellationToken ct)
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
        Func<UltraLiteDatabase, T> action,
        CancellationToken ct
    )
    {
        await _asyncSemaphore.WaitAsync(ct);

        try
        {
            return action(_database);
        }
        finally
        {
            _asyncSemaphore.Release();
        }
    }
}
