using System.Runtime.CompilerServices;
using Gaia.Services;
using Nestor.Db.Models;
using Nestor.Db.Services;

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
        var database = await _factory.CreateAsync(ct);

        var obj = await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetObjectEntityCollection();
                var document = collection.FindById(key);

                if (document is null)
                {
                    return new();
                }

                var obj = document.ToObjectEntity();

                return obj;
            },
            ct
        );

        if (obj.Content.Length == 0)
        {
            return new();
        }

        await using var stream = new MemoryStream(obj.Content);
        stream.Position = 0;

        return await _serializer.DeserializeAsync<T>(stream, ct) ?? new();
    }

    private async ValueTask SaveCore(string key, object obj, CancellationToken ct)
    {
        var database = await _factory.CreateAsync(ct);
        await using var stream = new MemoryStream();
        await _serializer.SerializeAsync(stream, obj, ct);
        stream.Position = 0;

        await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetObjectEntityCollection();
                var document = collection.FindById(key);

                var entity = new ObjectEntity
                {
                    Key = key,
                    Content = stream.ToArray(),
                    ContentType = _serializer.FileExtension,
                };

                if (document is null)
                {
                    collection.Insert(entity.ToBsonDocument());

                    return;
                }

                collection.Update(entity.ToBsonDocument());
            },
            ct
        );
    }
}
