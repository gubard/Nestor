using Gaia.Services;

namespace Nestor.Db.Models;

public class DbServiceOptionsFactory : IFactory<DbServiceOptions>
{
    private readonly DbServiceOptions _options;

    public DbServiceOptionsFactory(DbServiceOptions options)
    {
        _options = options;
    }

    public DbServiceOptions Create()
    {
        return _options;
    }
}
