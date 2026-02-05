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
    ConfiguredValueTaskAwaitable<EventEntity[]> GetEventsAsync(CancellationToken ct);
    ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct);
}

public abstract class DbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    : IDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, IPostResponse, new()
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

    protected readonly IDbConnectionFactory Factory;

    protected abstract ConfiguredValueTaskAwaitable<TPostResponse> ExecuteAsync(
        Guid idempotentId,
        TPostResponse response,
        TPostRequest request,
        CancellationToken ct
    );

    protected DbService(IDbConnectionFactory factory, params string[] eventEntityTypes)
    {
        Factory = factory;
        _eventEntityTypes = eventEntityTypes.ToArray();
    }

    private async ValueTask ClearEventsCore(CancellationToken ct)
    {
        await Factory.ExecuteNonQueryAsync(EventsExt.DeleteQuery, ct);
    }

    private readonly string[] _eventEntityTypes;

    private async ValueTask<TPostResponse> PostCore(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    )
    {
        var response = new TPostResponse();

        if (request.Events.Length == 0)
        {
            return await ExecuteAsync(idempotentId, response, request, ct);
        }

        await using var session = await Factory.CreateSessionAsync(ct);

        foreach (var e in request.Events)
        {
            var selectQuery = new SqlQuery(
                $"SELECT Id FROM {e.GetTableName()} WHERE Id = @Id",
                session.CreateParameter("@Id", e.EntityId)
            );

            var ids = await session.GetGuidAsync(selectQuery, ct);

            if (ids.Length == 0)
            {
                var insetQuery = InsertHelper.CreateDefaultInsert(
                    e.EntityType,
                    e.EntityId,
                    session
                );

                await session.ExecuteNonQueryAsync(insetQuery, ct);
            }

            var query = e.ToSqlQuery(session);
            await session.ExecuteNonQueryAsync(query, ct);
            await session.ExecuteNonQueryAsync(new[] { e }.CreateInsertQuery(session), ct);
        }

        await session.CommitAsync(ct);
        response.IsEventSaved = true;

        return await ExecuteAsync(idempotentId, response, request, ct);
    }

    private async ValueTask<EventEntity[]> GetEventsCore(CancellationToken ct)
    {
        await using var session = await Factory.CreateSessionAsync(ct);

        await using var reader = await session.ExecuteReaderAsync(
            new(
                EventsExt.SelectQuery
                    + $" WHERE EntityType IN ({_eventEntityTypes.ToParameterNames("EntityType")})",
                session.ToDbParameters(_eventEntityTypes, "EntityType")
            ),
            ct
        );

        var events = await reader.ReadEventsAsync(ct).ToEnumerableAsync();

        return events.ToArray();
    }
}
