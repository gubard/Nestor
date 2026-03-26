using UltraLiteDB;

namespace Nestor.Db.LiteDb.Helpers;

public static class DefaultBsonDocument
{
    private static readonly Dictionary<string, Func<Guid, BsonDocument>> Factories = new();

    public static BsonDocument CreateDefaultBsonDocument(string collectionName, Guid id)
    {
        return Factories[collectionName].Invoke(id);
    }

    public static void AddDefaultBsonDocument(string entityName, Func<Guid, BsonDocument> func)
    {
        Factories.Add(entityName, func);
    }
}
