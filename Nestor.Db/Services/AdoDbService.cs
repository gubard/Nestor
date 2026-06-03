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

public abstract class EmptyDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    : IDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
    where TGetResponse : IValidationErrors, new()
    where TPostResponse : IValidationErrors, IPostResponse, new()
    where TPostRequest : IPostRequest
{
    public ConfiguredValueTaskAwaitable<TGetResponse> GetAsync(
        TGetRequest request,
        CancellationToken ct
    )
    {
        return TaskHelper.FromResult(new TGetResponse());
    }

    public ConfiguredValueTaskAwaitable<TPostResponse> PostAsync(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    )
    {
        return TaskHelper.FromResult(new TPostResponse());
    }

    public ConfiguredValueTaskAwaitable<EventEntity[]> GetEventsAsync(CancellationToken ct)
    {
        return TaskHelper.FromResult(Array.Empty<EventEntity>());
    }

    public ConfiguredValueTaskAwaitable ClearEventsAsync(CancellationToken ct)
    {
        return TaskHelper.ConfiguredCompletedTask;
    }
}

public abstract class AdoDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
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

    protected readonly IAdoDatabaseFactory Factory;

    protected abstract ConfiguredValueTaskAwaitable ExecuteAsync(
        Guid idempotentId,
        TPostResponse response,
        TPostRequest request,
        CancellationToken ct
    );

    protected AdoDbService(IAdoDatabaseFactory factory, params string[] eventEntityTypes)
    {
        Factory = factory;
        _eventEntityTypes = eventEntityTypes.ToArray();
    }

    private readonly string[] _eventEntityTypes;

    public async ValueTask<EventEntity[]> GetEventsCore(CancellationToken ct)
    {
        var database = await Factory.CreateAsync(ct);

        return await database.ExecuteAsync(
            async command =>
            {
                await using var reader = await command.ExecuteReaderAsync(
                    new SqlQuery(
                        EventsExt.SelectQuery
                            + $" WHERE EntityType IN ({_eventEntityTypes.ToParameterNames("EntityType")})",
                        _eventEntityTypes.ToQueryParameters("EntityType")
                    ),
                    ct
                );

                var events = await reader.ReadEventsAsync(ct).ToEnumerableAsync();

                return events.ToArray();
            },
            ct
        );
    }

    private async ValueTask ClearEventsCore(CancellationToken ct)
    {
        var database = await Factory.CreateAsync(ct);

        await database.ExecuteAsync(c => c.ExecuteNonQueryAsync(EventsExt.DeleteQuery, ct), ct);
    }

    private async ValueTask<TPostResponse> PostCore(
        Guid idempotentId,
        TPostRequest request,
        CancellationToken ct
    )
    {
        var response = new TPostResponse();

        if (request.Events.Length == 0)
        {
            await ExecuteAsync(idempotentId, response, request, ct);

            return response;
        }

        var database = await Factory.CreateAsync(ct);

        await database.ExecuteAsync(
            async command =>
            {
                foreach (var e in request.Events)
                {
                    var selectQuery = new SqlQuery(
                        $"SELECT Id FROM {e.GetTableName()} WHERE Id = @Id",
                        new QueryParameter("@Id", e.EntityId)
                    );

                    var ids = await command.GetGuidAsync(selectQuery, ct);

                    if (ids.Length == 0)
                    {
                        var insetQuery = InsertHelper.CreateDefaultInsert(e.EntityType, e.EntityId);
                        await command.ExecuteNonQueryAsync(insetQuery, ct);
                    }

                    var query = e.ToSqlQuery();
                    await command.ExecuteNonQueryAsync(query, ct);
                    await command.ExecuteNonQueryAsync(new[] { e }.CreateInsertQuery(), ct);
                }
            },
            ct
        );

        response.IsEventSaved = true;
        await ExecuteAsync(idempotentId, response, request, ct);

        return response;
    }
}
