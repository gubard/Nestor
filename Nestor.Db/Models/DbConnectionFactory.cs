using System.Data.Common;
using Gaia.Services;
using Microsoft.Data.Sqlite;

namespace Nestor.Db.Models;

public interface IDbConnectionFactory : IFactory<DbConnection>;

public sealed class SqliteDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteDbConnectionFactory(SqliteConnectionStringBuilder builder)
    {
        _connectionString = builder.ConnectionString;
    }

    public DbConnection Create()
    {
        return new SqliteConnection(_connectionString);
    }
}
