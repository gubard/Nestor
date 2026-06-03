using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using Gaia.Helpers;

namespace Nestor.Db.Services;

public interface IDatabaseFactory<TDb>
{
    ConfiguredValueTaskAwaitable<IDatabase<TDb>> CreateAsync(CancellationToken ct);
}

public interface IAdoDatabaseFactory : IDatabaseFactory<DbCommand>;

public sealed class ValueDatabaseFactory<TDb> : IDatabaseFactory<TDb>
{
    public ValueDatabaseFactory(IDatabase<TDb> database)
    {
        _database = database;
    }

    public ConfiguredValueTaskAwaitable<IDatabase<TDb>> CreateAsync(CancellationToken ct)
    {
        return TaskHelper.FromResult(_database);
    }

    private readonly IDatabase<TDb> _database;
}
