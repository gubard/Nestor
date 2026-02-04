using System.Data.Common;
using System.IO;
using Gaia.Services;
using Microsoft.Data.Sqlite;

namespace Nestor.Db.Models;

public interface IDbConnectionFactory : IFactory<DbConnection>;

public sealed class SqliteDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteDbConnectionFactory(FileInfo file)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = file.FullName,
        }.ConnectionString;
    }

    public DbConnection Create()
    {
        return new SqliteConnection(_connectionString);
    }
}
