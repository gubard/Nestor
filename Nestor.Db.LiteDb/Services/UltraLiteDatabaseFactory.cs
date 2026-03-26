using Gaia.Services;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public interface IUltraLiteDatabaseFactory : IFactory<UltraLiteDatabase>;

public sealed class FileUltraLiteDatabaseFactory : IUltraLiteDatabaseFactory
{
    public FileUltraLiteDatabaseFactory(FileInfo file)
    {
        _file = file;
    }

    public UltraLiteDatabase Create()
    {
        return new(_file.FullName);
    }

    private readonly FileInfo _file;
}

public sealed class StreamUltraLiteDatabaseFactory : IUltraLiteDatabaseFactory
{
    public StreamUltraLiteDatabaseFactory(Stream stream)
    {
        _stream = stream;
    }

    public UltraLiteDatabase Create()
    {
        return new(_stream);
    }

    private readonly Stream _stream;
}
