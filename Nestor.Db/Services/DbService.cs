using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gaia.Helpers;
using Gaia.Services;
using Nestor.Db.Helpers;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface IDbService<in TGetRequest, in TPostRequest, TGetResponse, TPostResponse>
    : IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    ConfiguredValueTaskAwaitable<EventEntity[]> GetEventsAsync(CancellationToken ct);
    ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct);
}

public abstract class DbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    : IDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
    where TPostRequest : IPostRequest
{
    public abstract ConfiguredValueTaskAwaitable<TGetResponse> GetAsync(
        TGetRequest request,
        CancellationToken ct
    );

    public ConfiguredValueTaskAwaitable<TPostResponse> PostAsync(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    )
    {
        return PostCore(idempotentId, request, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable<EventEntity[]> GetEventsAsync(CancellationToken ct)
    {
        return GetEventsCore(ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct)
    {
        return ClearEventsCore(ct).ConfigureAwait(false);
    }

    public async ValueTask ClearEventsCore(CancellationToken ct)
    {
        await Factory.ExecuteNonQueryAsync(EventsExt.DeleteQuery, ct);
    }

    protected readonly IDbConnectionFactory Factory;

    protected abstract ConfiguredValueTaskAwaitable<TPostResponse> ExecuteAsync(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    );

    protected DbService(IDbConnectionFactory factory)
    {
        Factory = factory;
    }

    private async ValueTask<TPostResponse> PostCore(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    )
    {
        if (request.Events.Length == 0)
        {
            return await ExecuteAsync(idempotentId, request, ct);
        }

        await using var session = await Factory.CreateSessionAsync(ct);

        foreach (var e in request.Events)
        {
            var query = e.ToSqlQuery();
            await session.ExecuteNonQueryAsync(query, ct);
            await session.ExecuteNonQueryAsync(new[] { e }.CreateInsertQuery(), ct);
        }

        await session.CommitAsync(ct);

        return await ExecuteAsync(idempotentId, request, ct);
    }

    private async ValueTask<EventEntity[]> GetEventsCore(CancellationToken ct)
    {
        await using var reader = await Factory.ExecuteReaderAsync(EventsExt.SelectQuery, ct);

        return (await reader.ReadEventsAsync(ct).ToEnumerableAsync()).ToArray();
    }
}
