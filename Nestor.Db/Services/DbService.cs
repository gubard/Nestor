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
    EventEntity[] GetEvents();
    ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct);
    void ClearEvents();
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

    public abstract TGetResponse Get(TGetRequest request);

    public ConfiguredValueTaskAwaitable<TPostResponse> PostAsync(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    )
    {
        return PostCore(idempotentId, request, ct).ConfigureAwait(false);
    }

    public TPostResponse Post(Guid idempotentId, TPostRequest request)
    {
        if (request.Events.Length == 0)
        {
            return Execute(idempotentId, request);
        }

        foreach (var e in request.Events)
        {
            var query = e.ToSqlQuery();
            Factory.ExecuteNonQuery(query);
            Factory.ExecuteNonQuery(new[] { e }.CreateInsertQuery());
        }

        return Execute(idempotentId, request);
    }

    public ConfiguredValueTaskAwaitable<EventEntity[]> GetEventsAsync(CancellationToken ct)
    {
        return GetEventsCore(ct).ConfigureAwait(false);
    }

    public EventEntity[] GetEvents()
    {
        using var reader = Factory.ExecuteReader(EventsExt.SelectQuery);

        return reader.ReadEvents().ToArray();
    }

    public ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct)
    {
        return ClearEventsCore(ct).ConfigureAwait(false);
    }

    public void ClearEvents()
    {
        Factory.ExecuteNonQuery(EventsExt.DeleteQuery);
    }

    public async ValueTask ClearEventsCore(CancellationToken ct)
    {
        await Factory.ExecuteNonQueryAsync(EventsExt.DeleteQuery, ct);
    }

    protected readonly IDbConnectionFactory Factory;

    protected abstract TPostResponse Execute(Guid idempotentId, TPostRequest request);

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

        foreach (var e in request.Events)
        {
            var query = e.ToSqlQuery();
            await Factory.ExecuteNonQueryAsync(query, ct);
            await Factory.ExecuteNonQueryAsync(new[] { e }.CreateInsertQuery(), ct);
        }

        return await ExecuteAsync(idempotentId, request, ct);
    }

    private async ValueTask<EventEntity[]> GetEventsCore(CancellationToken ct)
    {
        await using var reader = await Factory.ExecuteReaderAsync(EventsExt.SelectQuery, ct);

        return (await reader.ReadEventsAsync(ct).ToEnumerableAsync()).ToArray();
    }
}
