using System.Runtime.CompilerServices;

namespace Nestor.Db.LiteDb.Services;

public interface IDatabaseFactory
{
    ConfiguredValueTaskAwaitable<IDatabase> CreateAsync(CancellationToken ct);
}
