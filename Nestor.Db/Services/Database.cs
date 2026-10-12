using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nestor.Db.Helpers;

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
    public AdoDatabase(IDbConnectionFactory factory, ILogger logger)
    {
        _factory = factory;
        _logger = logger;
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
    private readonly ILogger _logger;

    private SemaphoreSlim CreateSemaphoreSlim()
    {
        return new(1, 1);
    }

    private async ValueTask<T> ExecuteCore<T>(
        Func<DbCommand, ConfiguredValueTaskAwaitable<T>> action,
        CancellationToken ct
    )
    {
        await using var connection = _factory.Create();
        var dbLock = _dbLocks.GetOrAdd(connection.ConnectionString, _ => CreateSemaphoreSlim());
        await dbLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            await connection.OpenAsync(ct).ConfigureAwait(false);

            await using var transaction = await connection
                .BeginTransactionAsync(ct)
                .ConfigureAwait(false);

            await using var command = connection.CreateCommand();

            T result;

            try
            {
                result = await action.Invoke(command);
            }
            catch
            {
                _logger.CommandError(command.CommandText);
                await transaction.RollbackAsync(ct).ConfigureAwait(false);

                throw;
            }

            await transaction.CommitAsync(ct).ConfigureAwait(false);

            return result;
        }
        catch
        {
            _logger.ConnectionError(connection.ConnectionString);

            throw;
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
        await dbLock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            await connection.OpenAsync(ct).ConfigureAwait(false);

            await using var transaction = await connection
                .BeginTransactionAsync(ct)
                .ConfigureAwait(false);

            await using var command = connection.CreateCommand();

            try
            {
                await action.Invoke(command);
            }
            catch
            {
                _logger.CommandError(command.CommandText);
                await transaction.RollbackAsync(ct).ConfigureAwait(false);

                throw;
            }

            await transaction.CommitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            _logger.ConnectionError(connection.ConnectionString);

            throw;
        }
        finally
        {
            dbLock.Release();
        }
    }
}
