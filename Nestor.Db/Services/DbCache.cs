using System.Runtime.CompilerServices;
using System.Threading;
using Gaia.Services;

namespace Nestor.Db.Services;

public interface IDbCache<in TPostRequest, in TGetResponse> : ICache<TPostRequest, TGetResponse>;

public abstract class EmptyDbCache<TPostRequest, TGetResponse>
    : EmptyCache<TPostRequest, TGetResponse>,
        IDbCache<TPostRequest, TGetResponse>;

public abstract class DbCache<TPostRequest, TGetResponse> : IDbCache<TPostRequest, TGetResponse>
{
    public abstract ConfiguredValueTaskAwaitable UpdateAsync(
        TPostRequest source,
        CancellationToken ct
    );

    public abstract ConfiguredValueTaskAwaitable UpdateAsync(
        TGetResponse source,
        CancellationToken ct
    );

    protected readonly IDbConnectionFactory Factory;

    protected DbCache(IDbConnectionFactory factory)
    {
        Factory = factory;
    }
}
