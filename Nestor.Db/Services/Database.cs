using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nestor.Db.Services;

public interface IDatabase<out TDb>
{
    ConfiguredValueTaskAwaitable ExecuteAsync(
        Func<TDb, ConfiguredValueTaskAwaitable> action,
        CancellationToken ct
    );

    ConfiguredValueTaskAwaitable<T> ExecuteAsync<T>(
        Func<TDb, ConfiguredValueTaskAwaitable<T>> action,
        CancellationToken ct
    );
}

public interface IAdoDatabase : IDatabase<DbCommand>;

public sealed class AdoDatabase : IAdoDatabase
{
    public AdoDatabase(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public ConfiguredValueTaskAwaitable ExecuteAsync(
        Func<DbCommand, ConfiguredValueTaskAwaitable> action,
        CancellationToken ct
    )
    {
        return ExecuteCore(action, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable<T> ExecuteAsync<T>(
        Func<DbCommand, ConfiguredValueTaskAwaitable<T>> action,
        CancellationToken ct
    )
    {
        return ExecuteCore(action, ct).ConfigureAwait(false);
    }

    private static ConcurrentDictionary<string, SemaphoreSlim> _dbLocks = [];
    private readonly IDbConnectionFactory _factory;

    private SemaphoreSlim CreateSemaphoreSlim()
    {
        return new SemaphoreSlim(1, 1);
    }

    private async ValueTask<T> ExecuteCore<T>(
        Func<DbCommand, ConfiguredValueTaskAwaitable<T>> action,
        CancellationToken ct
    )
    {
        await using var connection = _factory.Create();
        var dbLock = _dbLocks.GetOrAdd(connection.ConnectionString, _ => CreateSemaphoreSlim());
        await dbLock.WaitAsync(ct);

        try
        {
            await connection.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);
            T result;

            try
            {
                result = await action.Invoke(connection.CreateCommand());
            }
            catch
            {
                await transaction.RollbackAsync(ct);

                throw;
            }

            await transaction.CommitAsync(ct);

            return result;
        }
        finally
        {
            dbLock.Release();
        }
    }

    private async ValueTask ExecuteCore(
        Func<DbCommand, ConfiguredValueTaskAwaitable> action,
        CancellationToken ct
    )
    {
        await using var connection = _factory.Create();
        var dbLock = _dbLocks.GetOrAdd(connection.ConnectionString, _ => CreateSemaphoreSlim());
        await dbLock.WaitAsync(ct);

        try
        {
            await connection.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);

            try
            {
                await action.Invoke(connection.CreateCommand());
            }
            catch
            {
                await transaction.RollbackAsync(ct);

                throw;
            }

            await transaction.CommitAsync(ct);
        }
        finally
        {
            dbLock.Release();
        }
    }
}
