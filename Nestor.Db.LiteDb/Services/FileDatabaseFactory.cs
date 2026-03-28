using System.Runtime.CompilerServices;
using Gaia.Helpers;

namespace Nestor.Db.LiteDb.Services;

public interface IDatabaseFactory
{
    ConfiguredValueTaskAwaitable<IDatabase> CreateAsync(CancellationToken ct);
}

public sealed class ValueDatabaseFactory : IDatabaseFactory
{
    public ValueDatabaseFactory(IDatabase database)
    {
        _database = database;
    }

    public ConfiguredValueTaskAwaitable<IDatabase> CreateAsync(CancellationToken ct)
    {
        return TaskHelper.FromResult(_database);
    }

    private readonly IDatabase _database;
}
