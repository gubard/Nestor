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
    ConfiguredValueTaskAwaitable AddEventsAsync(EventEntity[] events, CancellationToken ct);
    void AddEvents(EventEntity[] events);
    ConfiguredValueTaskAwaitable<EventEntity[]> GetEventsAsync(CancellationToken ct);
    EventEntity[] GetEvents();
    ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct);
    void ClearEvents();
}

public abstract class DbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    : IDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    public abstract ConfiguredValueTaskAwaitable<TGetResponse> GetAsync(
        TGetRequest request,
        CancellationToken ct
    );

    public abstract ConfiguredValueTaskAwaitable<TPostResponse> PostAsync(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    );

    public abstract TPostResponse Post(Guid idempotentId, TPostRequest request);
    public abstract TGetResponse Get(TGetRequest request);

    public ConfiguredValueTaskAwaitable AddEventsAsync(EventEntity[] events, CancellationToken ct)
    {
        return AddEventsCore(events, ct).ConfigureAwait(false);
    }

    public void AddEvents(EventEntity[] events)
    {
        if (events.Length == 0)
        {
            return;
        }

        Factory.ExecuteNonQuery(events.CreateInsertQuery());
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

    protected DbService(IDbConnectionFactory factory)
    {
        Factory = factory;
    }

    private async ValueTask<EventEntity[]> GetEventsCore(CancellationToken ct)
    {
        await using var reader = await Factory.ExecuteReaderAsync(EventsExt.SelectQuery, ct);

        return (await reader.ReadEventsAsync(ct).ToEnumerableAsync()).ToArray();
    }

    private async ValueTask AddEventsCore(EventEntity[] events, CancellationToken ct)
    {
        if (events.Length == 0)
        {
            return;
        }

        await Factory.ExecuteNonQueryAsync(events.CreateInsertQuery(), ct);
    }
}
