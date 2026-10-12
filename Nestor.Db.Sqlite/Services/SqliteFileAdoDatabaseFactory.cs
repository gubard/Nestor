using Microsoft.Extensions.Logging;
using Nestor.Db.Services;

namespace Nestor.Db.Sqlite.Services;

public sealed class SqliteFileAdoDatabaseFactory : IFileAdoDatabaseFactory
{
    public SqliteFileAdoDatabaseFactory(ILogger logger)
    {
        _logger = logger;
    }

    public IAdoDatabase Create(FileInfo input)
    {
        return new AdoDatabase(new FileSqliteDbConnectionFactory(input), _logger);
    }

    private readonly ILogger _logger;
}
