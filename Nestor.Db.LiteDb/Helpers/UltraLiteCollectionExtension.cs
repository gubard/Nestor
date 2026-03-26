using UltraLiteDB;

namespace Nestor.Db.LiteDb.Helpers;

public static class UltraLiteCollectionExtension
{
    public static bool Update(
        this UltraLiteCollection<BsonDocument> collection,
        BsonValue id,
        IReadOnlyDictionary<string, BsonValue> values
    )
    {
        var document = collection.FindById(id);

        if (document is null)
        {
            return false;
        }

        foreach (var value in values)
        {
            document[value.Key] = value.Value;
        }

        return collection.Update(document);
    }
}
