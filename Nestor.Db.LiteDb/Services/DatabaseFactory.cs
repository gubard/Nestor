using System.Runtime.CompilerServices;
using Gaia.Helpers;

namespace Nestor.Db.LiteDb.Services;

public interface IDatabaseFactory
{
    ConfiguredValueTaskAwaitable<IDatabase> CreateAsync(CancellationToken ct);
}

public sealed class DatabaseFactory : IDatabaseFactory
{
    public DatabaseFactory(IUltraLiteDatabaseFactory factory)
    {
        _factory = factory;
    }

    public ConfiguredValueTaskAwaitable<IDatabase> CreateAsync(CancellationToken ct)
    {
        return TaskHelper.FromResult((IDatabase)new Database(_factory.Create()));
    }

    private readonly IUltraLiteDatabaseFactory _factory;
}
