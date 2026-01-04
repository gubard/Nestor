using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface IEfService<in TGetRequest, in TPostRequest, TGetResponse, TPostResponse>
    : IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    ConfiguredValueTaskAwaitable SaveEventsAsync(
        ReadOnlyMemory<EventEntity> events,
        CancellationToken ct
    );

    void SaveEvents(ReadOnlyMemory<EventEntity> events);
    ConfiguredValueTaskAwaitable<long> GetLastIdAsync(CancellationToken ct);
    long GetLastId();
}

public abstract class EfService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    : IEfService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    protected readonly NestorDbContext DbContext;

    protected EfService(NestorDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public abstract ConfiguredValueTaskAwaitable<TGetResponse> GetAsync(
        TGetRequest request,
        CancellationToken ct
    );

    public abstract ConfiguredValueTaskAwaitable<TPostResponse> PostAsync(
        TPostRequest request,
        CancellationToken ct
    );

    public abstract TPostResponse Post(TPostRequest request);
    public abstract TGetResponse Get(TGetRequest request);

    public ConfiguredValueTaskAwaitable SaveEventsAsync(
        ReadOnlyMemory<EventEntity> events,
        CancellationToken ct
    )
    {
        return SaveEventsCore(events, ct).ConfigureAwait(false);
    }

    private async ValueTask SaveEventsCore(ReadOnlyMemory<EventEntity> events, CancellationToken ct)
    {
        if (events.IsEmpty)
        {
            return;
        }

        await DbContext.AddRangeAsync(events.ToArray(), ct);
        await DbContext.SaveChangesAsync(ct);
    }

    public void SaveEvents(ReadOnlyMemory<EventEntity> events)
    {
        if (events.IsEmpty)
        {
            return;
        }

        DbContext.AddRange(events.ToArray());
        DbContext.SaveChanges();
    }

    public ConfiguredValueTaskAwaitable<long> GetLastIdAsync(CancellationToken ct)
    {
        return GetLastIdCore(ct).ConfigureAwait(false);
    }

    private async ValueTask<long> GetLastIdCore(CancellationToken ct)
    {
        var lastId = await DbContext.Events.MaxAsync(x => (long?)x.Id, ct);

        if (lastId is null)
        {
            return 0;
        }

        return lastId.Value;
    }

    public long GetLastId()
    {
        var lastId = DbContext.Events.Max(x => (long?)x.Id);

        if (lastId is null)
        {
            return 0;
        }

        return lastId.Value;
    }
}
