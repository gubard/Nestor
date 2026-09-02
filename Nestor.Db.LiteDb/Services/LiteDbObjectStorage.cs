using System.Runtime.CompilerServices;
using Gaia.Helpers;
using Gaia.Services;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Nestor.Db.LiteDb.Services;

public sealed class LiteDbObjectStorage : IObjectStorage
{
    public LiteDbObjectStorage(IUltraLiteDatabaseFactory factory, ISerializer serializer)
    {
        _factory = factory;
        _serializer = serializer;
    }

    public ConfiguredValueTaskAwaitable<T> LoadAsync<T>(string key, CancellationToken ct)
        where T : IStaticFactory<T>
    {
        return LoadCore<T>(key, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable SaveAsync(string key, object obj, CancellationToken ct)
    {
        return SaveCore(key, obj, ct).ConfigureAwait(false);
    }

    private readonly IUltraLiteDatabaseFactory _factory;
    private readonly ISerializer _serializer;

    private async ValueTask<T> LoadCore<T>(string key, CancellationToken ct)
        where T : IStaticFactory<T>
    {
        var database = await _factory.CreateAsync(ct);

        var obj = await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetObjectEntityCollection();
                var document = collection.FindById(key);

                if (document is null)
                {
                    return TaskHelper.FromResult(new ObjectEntity());
                }

                var obj = document.ToObjectEntity();

                return TaskHelper.FromResult(obj);
            },
            ct
        );

        if (obj.Content.Length == 0)
        {
            return T.Create();
        }

        await using var stream = new MemoryStream(obj.Content);
        stream.Position = 0;

        return await _serializer.DeserializeAsync<T>(stream, ct) ?? T.Create();
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

                    return TaskHelper.ConfiguredCompletedTask;
                }

                collection.Update(entity.ToBsonDocument());

                return TaskHelper.ConfiguredCompletedTask;
            },
            ct
        );
    }
}
