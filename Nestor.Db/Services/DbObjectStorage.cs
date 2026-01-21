using System.IO;
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

public sealed class DbObjectStorage : IObjectStorage
{
    public DbObjectStorage(IDbConnectionFactory factory, ISerializer serializer)
    {
        _factory = factory;
        _serializer = serializer;
    }

    public ConfiguredValueTaskAwaitable<T?> LoadAsync<T>(string key, CancellationToken ct)
    {
        return LoadCore<T>(key, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable SaveAsync(string key, object obj, CancellationToken ct)
    {
        return SaveCore(key, obj, ct).ConfigureAwait(false);
    }

    private readonly IDbConnectionFactory _factory;
    private readonly ISerializer _serializer;

    private async ValueTask<T?> LoadCore<T>(string key, CancellationToken ct)
    {
        await using var reader = await _factory.ExecuteReaderAsync(
            new(ObjectsExt.SelectQuery + " WHERE Key = @Key", new SqliteParameter("@Key", key)),
            ct
        );

        var objs = (await reader.ReadObjectsAsync(ct).ToEnumerableAsync()).ToArray();

        if (objs.Length == 0)
        {
            return default;
        }

        await using var stream = new MemoryStream(objs[0].Content);
        stream.Position = 0;

        return await _serializer.DeserializeAsync<T>(stream, ct);
    }

    private async ValueTask SaveCore(string key, object obj, CancellationToken ct)
    {
        var count = await _factory.ExecuteScalarInt32Async(
            new(
                ObjectsExt.SelectCountQuery + " WHERE Key = @Key",
                new SqliteParameter("@Key", key)
            ),
            ct
        );

        await using var stream = new MemoryStream();
        await _serializer.SerializeAsync(stream, obj, ct);
        stream.Position = 0;

        var entity = new ObjectEntity
        {
            Key = key,
            Content = stream.ToArray(),
            ContentType = _serializer.FileExtension,
        };

        if (count == 0)
        {
            await _factory.ExecuteNonQueryAsync(new[] { entity }.CreateInsertQuery(), ct);
        }

        await _factory.ExecuteNonQueryAsync(entity.CreateUpdateObjectsQuery(), ct);
    }
}
