using System.Runtime.CompilerServices;
using Gaia.Helpers;
using Gaia.Services;
using Nestor.Db.Services;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public interface IUltraLiteDatabaseFactory : IDatabaseFactory<UltraLiteDatabase>;

public sealed class FileUltraLiteDatabaseFactory : IUltraLiteDatabaseFactory
{
    public FileUltraLiteDatabaseFactory(FileInfo file)
    {
        _file = file;
    }

    public IUltraLiteDatabase Create()
    {
        return new Database(new(new ConnectionString(_file.FullName)));
    }

    private readonly FileInfo _file;

    public ConfiguredValueTaskAwaitable<IDatabase<UltraLiteDatabase>> CreateAsync(
        CancellationToken ct
    )
    {
        return TaskHelper.FromResult<IDatabase<UltraLiteDatabase>>(
            new Database(new(new ConnectionString(_file.FullName)))
        );
    }
}

public sealed class StreamUltraLiteDatabaseFactory : IUltraLiteDatabaseFactory
{
    public StreamUltraLiteDatabaseFactory(Stream stream)
    {
        _stream = stream;
    }

    public ConfiguredValueTaskAwaitable<IDatabase<UltraLiteDatabase>> CreateAsync(
        CancellationToken ct
    )
    {
        return TaskHelper.FromResult<IDatabase<UltraLiteDatabase>>(new Database(new(_stream)));
    }

    private readonly Stream _stream;
}
