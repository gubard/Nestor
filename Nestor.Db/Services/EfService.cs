using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface
    IEfService<in TGetRequest, in TPostRequest, TGetResponse, TPostResponse> :
    IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    ValueTask SaveEventsAsync(ReadOnlyMemory<EventEntity> events,
        CancellationToken ct);

    void SaveEvents(ReadOnlyMemory<EventEntity> events);
    ValueTask<long> GetLastIdAsync(CancellationToken ct);
    long GetLastId();
}

public abstract class
    EfService<TGetRequest, TPostRequest, TGetResponse, TPostResponse> :
    IEfService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    protected readonly DbContext DbContext;

    protected EfService(DbContext dbContext)
    {
        DbContext = dbContext;
    }

    public abstract ValueTask<TGetResponse> GetAsync(TGetRequest request,
        CancellationToken ct);

    public abstract ValueTask<TPostResponse> PostAsync(TPostRequest request,
        CancellationToken ct);

    public abstract TPostResponse Post(TPostRequest request);

    public async ValueTask SaveEventsAsync(ReadOnlyMemory<EventEntity> events,
        CancellationToken ct)
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

    public async ValueTask<long> GetLastIdAsync(CancellationToken ct)
    {
        var lastId = await DbContext.Set<EventEntity>()
           .MaxAsync(x => (long?)x.Id, ct);

        if (lastId is null)
        {
            return 0;
        }

        return lastId.Value;
    }

    public long GetLastId()
    {
        var lastId = DbContext.Set<EventEntity>()
           .Max(x => (long?)x.Id);

        if (lastId is null)
        {
            return 0;
        }

        return lastId.Value;
    }
}