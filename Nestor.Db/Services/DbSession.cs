using System;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public readonly struct DbSession : IDisposable, IAsyncDisposable
{
    private DbSession(DbConnection connection, DbCommand command, DbTransaction transaction)
    {
        _connection = connection;
        _command = command;
        _transaction = transaction;
    }

    public DbDataReader ExecuteReader(SqlQuery query)
    {
        try
        {
            _command.CommandText = query.Sql;
            _command.Parameters.Clear();
            _command.Parameters.AddRange(query.Parameters);

            return _command.ExecuteReader();
        }
        catch
        {
            Rollback();

            throw;
        }
    }

    public ConfiguredValueTaskAwaitable<DbDataReader> ExecuteReaderAsync(
        SqlQuery query,
        CancellationToken ct
    )
    {
        return ExecuteReaderCore(query, ct).ConfigureAwait(false);
    }

    public int ExecuteNonQuery(SqlQuery query)
    {
        try
        {
            _command.CommandText = query.Sql;
            _command.Parameters.Clear();
            _command.Parameters.AddRange(query.Parameters);
            var result = _command.ExecuteNonQuery();

            return Convert.ToInt32(result);
        }
        catch
        {
            Rollback();

            throw;
        }
    }

    public ConfiguredValueTaskAwaitable<int> ExecuteNonQueryAsync(
        SqlQuery query,
        CancellationToken ct
    )
    {
        return ExecuteNonQueryCore(query, ct).ConfigureAwait(false);
    }

    public int ExecuteScalarInt32(SqlQuery query)
    {
        try
        {
            _command.CommandText = query.Sql;
            _command.Parameters.Clear();
            _command.Parameters.AddRange(query.Parameters);
            var result = _command.ExecuteScalar();

            return Convert.ToInt32(result);
        }
        catch
        {
            Rollback();

            throw;
        }
    }

    public ConfiguredValueTaskAwaitable<int> ExecuteScalarInt32Async(
        SqlQuery query,
        CancellationToken ct
    )
    {
        return ExecuteScalarInt32Core(query, ct).ConfigureAwait(false);
    }

    public void Commit()
    {
        _transaction.Commit();
    }

    public ConfiguredValueTaskAwaitable CommitAsync(CancellationToken ct)
    {
        return CommitCore(ct).ConfigureAwait(false);
    }

    public void Rollback()
    {
        _transaction.Rollback();
    }

    public ConfiguredValueTaskAwaitable RollbackAsync(CancellationToken ct)
    {
        return RollbackCore(ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _command.Dispose();
        _transaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _command.DisposeAsync();
        await _transaction.DisposeAsync();
    }

    public static DbSession Create(DbConnection connection)
    {
        var command = connection.CreateCommand();
        connection.Open();
        command.Transaction = connection.BeginTransaction();

        return new(connection, command, command.Transaction);
    }

    public static ConfiguredValueTaskAwaitable<DbSession> CreateAsync(
        DbConnection connection,
        CancellationToken ct
    )
    {
        return CreateCore(connection, ct).ConfigureAwait(false);
    }

    private readonly DbConnection _connection;
    private readonly DbCommand _command;
    private readonly DbTransaction _transaction;

    private async ValueTask<DbDataReader> ExecuteReaderCore(SqlQuery query, CancellationToken ct)
    {
        try
        {
            _command.CommandText = query.Sql;
            _command.Parameters.Clear();
            _command.Parameters.AddRange(query.Parameters);

            return await _command.ExecuteReaderAsync(ct);
        }
        catch
        {
            await RollbackAsync(ct);

            throw;
        }
    }

    private async ValueTask<int> ExecuteNonQueryCore(SqlQuery query, CancellationToken ct)
    {
        try
        {
            _command.CommandText = query.Sql;
            _command.Parameters.Clear();
            _command.Parameters.AddRange(query.Parameters);

            return await _command.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            await RollbackAsync(ct);

            throw;
        }
    }

    private async ValueTask<int> ExecuteScalarInt32Core(SqlQuery query, CancellationToken ct)
    {
        try
        {
            _command.CommandText = query.Sql;
            _command.Parameters.Clear();
            _command.Parameters.AddRange(query.Parameters);
            var result = await _command.ExecuteScalarAsync(ct);

            return Convert.ToInt32(result);
        }
        catch
        {
            await RollbackAsync(ct);

            throw;
        }
    }

    private async ValueTask RollbackCore(CancellationToken ct)
    {
        await _transaction.RollbackAsync(ct);
    }

    private async ValueTask CommitCore(CancellationToken ct)
    {
        await _transaction.CommitAsync(ct);
    }

    private static async ValueTask<DbSession> CreateCore(
        DbConnection connection,
        CancellationToken ct
    )
    {
        var command = connection.CreateCommand();
        await connection.OpenAsync(ct);
        command.Transaction = await connection.BeginTransactionAsync(ct);

        return new(connection, command, command.Transaction);
    }
}
