using System.Runtime.CompilerServices;
using Gaia.Helpers;
using Gaia.Services;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Nestor.Db.LiteDb.Services;

public sealed class LiteDbIdempotenceService : IIdempotenceService
{
    public LiteDbIdempotenceService(IUltraLiteDatabaseFactory factory, ISerializer serializer)
    {
        _factory = factory;
        _serializer = serializer;
    }

    public ConfiguredValueTaskAwaitable<T?> GetAsync<T>(Guid id, CancellationToken ct)
        where T : class
    {
        return GetCore<T>(id, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable AddAsync(Guid id, object value, CancellationToken ct)
    {
        return AddCore(id, value, ct).ConfigureAwait(false);
    }

    private readonly IUltraLiteDatabaseFactory _factory;
    private readonly ISerializer _serializer;

    private async ValueTask AddCore(Guid id, object value, CancellationToken ct)
    {
        var database = await _factory.CreateAsync(ct);
        await using var stream = new MemoryStream();
        await _serializer.SerializeAsync(stream, value, ct);
        stream.Position = 0;

        var item = new IdempotentEntity
        {
            Data = stream.ToArray(),
            CreatedAt = DateTimeOffset.UtcNow,
            DataType = _serializer.FileExtension,
            Id = id,
        };

        var document = item.ToBsonDocument();

        await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetIdempotentEntityCollection();
                collection.Insert(document);

                return TaskHelper.ConfiguredCompletedTask;
            },
            ct
        );
    }

    private async ValueTask<T?> GetCore<T>(Guid id, CancellationToken ct)
        where T : class
    {
        var database = await _factory.CreateAsync(ct);

        var document = await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetIdempotentEntityCollection();

                return TaskHelper.FromResult(collection.FindById(id));
            },
            ct
        );

        if (document is null)
        {
            return null;
        }

        var item = document.ToIdempotentEntity();
        await using var stream = new MemoryStream(item.Data);
        stream.Position = 0;

        return await _serializer.DeserializeAsync<T>(stream, ct);
    }
}
