using System.Runtime.CompilerServices;
using Gaia.Models;
using Gaia.Services;
using Nestor.Db.Models;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public sealed class LiteDbObjectStorage : IObjectStorage
{
    public LiteDbObjectStorage(IDatabaseFactory factory, ISerializer serializer)
    {
        _factory = factory;
        _serializer = serializer;
    }

    public ConfiguredValueTaskAwaitable<T> LoadAsync<T>(string key, CancellationToken ct)
        where T : new()
    {
        return LoadCore<T>(key, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable SaveAsync(string key, object obj, CancellationToken ct)
    {
        return SaveCore(key, obj, ct).ConfigureAwait(false);
    }

    private readonly IDatabaseFactory _factory;
    private readonly ISerializer _serializer;

    private async ValueTask<T> LoadCore<T>(string key, CancellationToken ct)
        where T : new()
    {
        using var database = await _factory.CreateAsync(ct);
        var collection = database.GetObjectEntityCollection();
        var document = collection.FindById(key);

        if (document is null)
        {
            return new();
        }

        var obj = document.ToObjectEntity();
        await using var stream = new MemoryStream(obj.Content);
        stream.Position = 0;

        return await _serializer.DeserializeAsync<T>(stream, ct) ?? new();
    }

    private async ValueTask SaveCore(string key, object obj, CancellationToken ct)
    {
        using var database = await _factory.CreateAsync(ct);
        var collection = database.GetObjectEntityCollection();
        var document = collection.FindById(key);
        await using var stream = new MemoryStream();
        await _serializer.SerializeAsync(stream, obj, ct);
        stream.Position = 0;

        var entity = new ObjectEntity
        {
            Key = key,
            Content = stream.ToArray(),
            ContentType = _serializer.FileExtension,
        };

        await UpdateDocument(database, collection, entity, document, ct);
    }

    private async ValueTask UpdateDocument(
        IDatabase database,
        UltraLiteCollection<BsonDocument> collection,
        ObjectEntity entity,
        BsonDocument? document,
        CancellationToken ct
    )
    {
        await using var fin = new FinallyAsync(async () => await database.SaveChangesAsync(ct));

        if (document is null)
        {
            collection.Insert(entity.ToBsonDocument());

            return;
        }

        collection.Update(entity.ToBsonDocument());
    }
}
