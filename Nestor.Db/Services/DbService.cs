using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gaia.Helpers;
using Gaia.Services;
using Microsoft.Data.Sqlite;
using Nestor.Db.Helpers;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public interface IDbService<in TGetRequest, in TPostRequest, TGetResponse, TPostResponse>
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

public abstract class DbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    : IDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, new()
{
    protected readonly IDbConnectionFactory Factory;

    protected DbService(IDbConnectionFactory factory)
    {
        Factory = factory;
    }

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

        await Factory.ExecuteNonQueryAsync(events.Span.CreateInsertQuery(), ct);
    }

    public void SaveEvents(ReadOnlyMemory<EventEntity> events)
    {
        if (events.IsEmpty)
        {
            return;
        }

        Factory.ExecuteNonQuery(events.Span.CreateInsertQuery());
    }

    public ConfiguredValueTaskAwaitable<long> GetLastIdAsync(CancellationToken ct)
    {
        return Factory.ExecuteScalarInt64Async("SELECT IFNULL(MAX(id), 0) FROM Events", ct);
    }

    public long GetLastId()
    {
        return Factory.ExecuteScalarInt64("SELECT IFNULL(MAX(id), 0) FROM Events");
    }

    protected ConfiguredValueTaskAwaitable<EventEntity[]> GetLastEventsAsync(
        DbSession session,
        long lastId,
        CancellationToken ct
    )
    {
        return GetLastEventsCore(session, lastId, ct).ConfigureAwait(false);
    }

    private async ValueTask<EventEntity[]> GetLastEventsCore(
        DbSession session,
        long lastId,
        CancellationToken ct
    )
    {
        await using var reader = await session.ExecuteReaderAsync(
            CreateLastEventsQuery(lastId),
            ct
        );

        return (await reader.ReadEventsAsync(ct).ToEnumerableAsync()).ToArray();
    }

    protected EventEntity[] GetLastEvents(DbSession session, long lastId)
    {
        using var reader = session.ExecuteReader(CreateLastEventsQuery(lastId));

        return reader.ReadEvents().ToArray();
    }

    private static SqlQuery CreateLastEventsQuery(long lastId)
    {
        return new(
            $"{EventsExt.SelectQuery} WHERE Id > @LastId",
            new SqliteParameter[] { new("@LastId", lastId) }
        );
    }
}
