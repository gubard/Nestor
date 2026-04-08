using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Nestor.Db.Services;

public interface IIdempotenceService
{
    ConfiguredValueTaskAwaitable<T?> GetAsync<T>(Guid id, CancellationToken ct)
        where T : class;

    ConfiguredValueTaskAwaitable AddAsync(Guid id, object value, CancellationToken ct);
}
