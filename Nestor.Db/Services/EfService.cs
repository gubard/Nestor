using System;
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

    ValueTask<long> GetLastIdAsync(CancellationToken ct);
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
}