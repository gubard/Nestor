using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Gaia.Services;
using Nestor.Db.Helpers;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public sealed class AdoObjectStorage : IObjectStorage
{
    public AdoObjectStorage(IAdoDatabaseFactory databaseFactory, ISerializer serializer)
    {
        _databaseFactory = databaseFactory;
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

    private readonly IAdoDatabaseFactory _databaseFactory;
    private readonly ISerializer _serializer;

    private async ValueTask SaveCore(string key, object obj, CancellationToken ct)
    {
        var database = await _databaseFactory.CreateAsync(ct);

        await database.ExecuteAsync(
            async command =>
            {
                var entity = await command.GetObjectsByIdAsync(key, ct);
                using var stream = new MemoryStream();
                await _serializer.SerializeAsync(stream, obj, ct);
                stream.Position = 0;

                if (entity is null)
                {
                    await command.ExecuteNonQueryAsync(
                        new ObjectEntity
                        {
                            Content = stream.ToArray(),
                            ContentType = _serializer.FileExtension,
                            Key = key,
                        }.CreateInsertQuery(),
                        ct
                    );
                }
                else
                {
                    await command.ExecuteNonQueryAsync(
                        new ObjectEntity[]
                        {
                            new()
                            {
                                Content = stream.ToArray(),
                                ContentType = _serializer.FileExtension,
                                Key = key,
                            },
                        }.CreateUpdateObjectsQuery(),
                        ct
                    );
                }
            },
            ct
        );
    }

    private async ValueTask<T> LoadCore<T>(string key, CancellationToken ct)
        where T : IStaticFactory<T>
    {
        var database = await _databaseFactory.CreateAsync(ct);

        return await database.ExecuteAsync(
            async command =>
            {
                var entity = await command.GetObjectsByIdAsync(key, ct);

                if (entity is null)
                {
                    return T.Create();
                }

                using var stream = new MemoryStream(entity.Content);
                stream.Position = 0;
                var value = await SafeDeserializeAsync<T>(stream, ct);

                return value ?? T.Create();
            },
            ct
        );
    }

    private async ValueTask<T?> SafeDeserializeAsync<T>(Stream stream, CancellationToken ct)
    {
        try
        {
            return await _serializer.DeserializeAsync<T>(stream, ct);
        }
        catch
        {
            return default;
        }
    }
}
