using System.Runtime.CompilerServices;
using System.Threading;

namespace Nestor.Db.Services;

public interface IDbCache<in TSource>
{
    ConfiguredValueTaskAwaitable UpdateAsync(TSource source, CancellationToken ct);
    void Update(TSource source);
}
