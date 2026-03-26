using UltraLiteDB;

namespace Nestor.Db.LiteDb.Services;

public interface IDatabase : IDisposable
{
    bool DropCollection(string name);
    UltraLiteCollection<BsonDocument> GetCollection(string name, BsonAutoId autoId);
    void SaveChanges();
}

public sealed class Database : IDatabase
{
    private readonly UltraLiteDatabase _database;

    public Database(UltraLiteDatabase database)
    {
        _database = database;
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    public bool DropCollection(string name)
    {
        return _database.DropCollection(name);
    }

    public UltraLiteCollection<BsonDocument> GetCollection(string name, BsonAutoId autoId)
    {
        return _database.GetCollection(name, autoId);
    }

    public void SaveChanges() { }
}
