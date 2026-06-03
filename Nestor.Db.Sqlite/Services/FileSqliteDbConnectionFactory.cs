using System.Data.Common;
using Microsoft.Data.Sqlite;
using Nestor.Db.Services;

namespace Nestor.Db.Sqlite.Services;

public sealed class FileSqliteDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public FileSqliteDbConnectionFactory(FileInfo file)
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
