using System;
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

public interface IIdempotenceService
{
    ConfiguredValueTaskAwaitable<T?> GetAsync<T>(Guid id, CancellationToken ct)
        where T : class;

    ConfiguredValueTaskAwaitable AddAsync(Guid id, object value, CancellationToken ct);
}

public sealed class IdempotenceService : IIdempotenceService
{
    public IdempotenceService(IDbConnectionFactory factory, ISerializer serializer)
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

    private readonly IDbConnectionFactory _factory;
    private readonly ISerializer _serializer;

    private async ValueTask AddCore(Guid id, object value, CancellationToken ct)
    {
        await using var session = await _factory.CreateSessionAsync(ct);
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

        await session.ExecuteNonQueryAsync(new[] { item }.CreateInsertQuery(), ct);
        await session.CommitAsync(ct);
    }

    private async ValueTask<T?> GetCore<T>(Guid id, CancellationToken ct)
        where T : class
    {
        await using var session = await _factory.CreateSessionAsync(ct);

        var query = new SqlQuery(
            IdempotentsExt.SelectQuery + " WHERE Id = @Id",
            session.CreateParameter("@Id", id)
        );

        await using var reader = await session.ExecuteReaderAsync(query, ct);
        var items = await reader.ReadIdempotentsAsync(ct).ToEnumerableAsync();
        var item = items.FirstOrDefault();

        if (item == null)
        {
            return null;
        }

        await using var stream = new MemoryStream(item.Data);
        stream.Position = 0;

        return await _serializer.DeserializeAsync<T>(stream, ct);
    }
}
