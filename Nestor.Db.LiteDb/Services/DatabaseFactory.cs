using Gaia.Services;

namespace Nestor.Db.LiteDb.Services;

public interface IDatabaseFactory : IFactory<IDatabase>;

public sealed class DatabaseFactory : IDatabaseFactory
{
    public DatabaseFactory(IUltraLiteDatabaseFactory factory)
    {
        _factory = factory;
    }

    public IDatabase Create()
    {
        return new Database(_factory.Create());
    }

    private readonly IUltraLiteDatabaseFactory _factory;
}
