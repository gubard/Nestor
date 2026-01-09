using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Nestor.Db.Helpers;

public static class DbConnectionFactoryExtension
{
    extension<TDbConnectionFactory>(TDbConnectionFactory factory)
        where TDbConnectionFactory : IDbConnectionFactory
    {
        public DbSession CreateSession()
        {
            return DbSession.Create(factory.Create());
        }

        public ConfiguredValueTaskAwaitable<DbSession> CreateSessionAsync(CancellationToken ct)
        {
            return DbSession.CreateAsync(factory.Create(), ct);
        }

        public bool IsCanConnect(SqlQuery query)
        {
            try
            {
                using var connection = factory.Create();
                using var command = connection.CreateCommand();
                command.CommandText = query.Sql;
                command.Parameters.Clear();
                command.Parameters.AddRange(query.Parameters);
                connection.Open();
                command.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public ConfiguredValueTaskAwaitable<bool> IsCanConnectAsync(
            SqlQuery query,
            CancellationToken ct
        )
        {
            return factory.IsCanConnectCore(query, ct).ConfigureAwait(false);
        }

        public long ExecuteScalarInt64(SqlQuery query)
        {
            using var connection = factory.Create();
            using var command = connection.CreateCommand();
            command.CommandText = query.Sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(query.Parameters);
            connection.Open();
            var result = command.ExecuteScalar();

            return Convert.ToInt64(result);
        }

        public ConfiguredValueTaskAwaitable<long> ExecuteScalarInt64Async(
            SqlQuery query,
            CancellationToken ct
        )
        {
            return factory.ExecuteScalarInt64Core(query, ct).ConfigureAwait(false);
        }

        public int ExecuteScalarInt32(SqlQuery query)
        {
            using var connection = factory.Create();
            using var command = connection.CreateCommand();
            command.CommandText = query.Sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(query.Parameters);
            connection.Open();
            var result = command.ExecuteScalar();

            return Convert.ToInt32(result);
        }

        public ConfiguredValueTaskAwaitable<int> ExecuteScalarInt32Async(
            SqlQuery query,
            CancellationToken ct
        )
        {
            return factory.ExecuteScalarInt32Core(query, ct).ConfigureAwait(false);
        }

        public int ExecuteNonQuery(SqlQuery query)
        {
            using var connection = factory.Create();
            using var command = connection.CreateCommand();
            command.CommandText = query.Sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(query.Parameters);
            connection.Open();

            return command.ExecuteNonQuery();
        }

        public ConfiguredValueTaskAwaitable<int> ExecuteNonQueryAsync(
            SqlQuery query,
            CancellationToken ct
        )
        {
            return factory.ExecuteNonQueryCore(query, ct).ConfigureAwait(false);
        }

        private async ValueTask<bool> IsCanConnectCore(SqlQuery query, CancellationToken ct)
        {
            try
            {
                await using var connection = factory.Create();
                await using var command = connection.CreateCommand();
                command.CommandText = query.Sql;
                command.Parameters.Clear();
                command.Parameters.AddRange(query.Parameters);
                await connection.OpenAsync(ct);
                await command.ExecuteNonQueryAsync(ct);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async ValueTask<int> ExecuteNonQueryCore(SqlQuery query, CancellationToken ct)
        {
            await using var connection = factory.Create();
            await using var command = connection.CreateCommand();
            command.CommandText = query.Sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(query.Parameters);
            await connection.OpenAsync(ct);
            var rowCount = await command.ExecuteNonQueryAsync(ct);

            return rowCount;
        }

        private async ValueTask<int> ExecuteScalarInt32Core(SqlQuery query, CancellationToken ct)
        {
            await using var connection = factory.Create();
            await using var command = connection.CreateCommand();
            command.CommandText = query.Sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(query.Parameters);
            await connection.OpenAsync(ct);
            var result = await command.ExecuteScalarAsync(ct);

            return Convert.ToInt32(result);
        }

        private async ValueTask<long> ExecuteScalarInt64Core(SqlQuery query, CancellationToken ct)
        {
            await using var connection = factory.Create();
            await using var command = connection.CreateCommand();
            command.CommandText = query.Sql;
            command.Parameters.Clear();
            command.Parameters.AddRange(query.Parameters);
            await connection.OpenAsync(ct);
            var result = await command.ExecuteScalarAsync(ct);

            return Convert.ToInt64(result);
        }
    }
}
