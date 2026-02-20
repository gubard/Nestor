using System.Data.Common;
using Microsoft.Data.Sqlite;
using Nestor.Db.Services;

namespace Nestor.Db.Sqlite.Services;

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
