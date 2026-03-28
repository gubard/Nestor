using System.Runtime.CompilerServices;
using Gaia.Services;
using Nestor.Db.LiteDb.Helpers;
using Nestor.Db.Models;
using Nestor.Db.Services;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public abstract class LiteDbService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
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

    protected readonly IDatabaseFactory Factory;

    protected abstract ConfiguredValueTaskAwaitable ExecuteAsync(
        Guid idempotentId,
        TPostResponse response,
        TPostRequest request,
        CancellationToken ct
    );

    protected LiteDbService(IDatabaseFactory factory, params string[] eventEntityTypes)
    {
        Factory = factory;
        _eventEntityTypes = eventEntityTypes.Select(x => new BsonValue(x)).ToArray();
    }

    private readonly BsonValue[] _eventEntityTypes;

    public async ValueTask ClearEventsCore(CancellationToken ct)
    {
        var database = await Factory.CreateAsync(ct);
        await database.ExecuteAsync(db => db.DropEventEntityCollection(), ct);
    }

    private async ValueTask<EventEntity[]> GetEventsCore(CancellationToken ct)
    {
        var database = await Factory.CreateAsync(ct);

        return await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetEventEntityCollection();
                var documents = collection.Find(Query.In("EntityType", _eventEntityTypes));

                if (documents is null)
                {
                    return [];
                }

                var events = documents.Select(x => x.ToEventEntity()).ToArray();

                return events;
            },
            ct
        );
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
            db =>
            {
                foreach (var e in request.Events)
                {
                    var eventCollection = db.GetEventEntityCollection();
                    var collection = db.GetCollection(e.GetEntityCollectionName(), BsonAutoId.Guid);
                    var entity = collection.FindById(e.EntityId);

                    if (entity is null)
                    {
                        var document = DefaultBsonDocument.CreateDefaultBsonDocument(
                            e.EntityType,
                            e.EntityId
                        );

                        collection.Insert(document);
                    }
                    else
                    {
                        entity[e.EntityProperty] = e.GetBsonValue();
                        collection.Update(entity);
                    }

                    var @event = e.ToBsonDocument();
                    @event.Remove("_id");
                    eventCollection.Insert(@event);
                }
            },
            ct
        );

        response.IsEventSaved = true;
        await ExecuteAsync(idempotentId, response, request, ct);

        return response;
    }
}
