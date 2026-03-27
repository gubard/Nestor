using UltraLiteDB;

namespace Nestor.Db.LiteDb.Helpers;

public static class BsonDocumentExtension
{
    public static DateTime? GetDateTime(this BsonDocument document, string key)
    {
        if (!document.TryGetValue(key, out var value))
        {
            return null;
        }

        if (value.IsDateTime)
        {
            return value;
        }

        return null;
    }

    public static DateTime GetDateTimeOrDefault(
        this BsonDocument document,
        string key,
        DateTime def
    )
    {
        if (!document.TryGetValue(key, out var value))
        {
            return def;
        }

        if (value.IsDateTime)
        {
            return value;
        }

        return def;
    }

    public static bool? GetBoolean(this BsonDocument document, string key)
    {
        return document.GetBool(key);
    }

    public static bool GetBooleanOrDefault(this BsonDocument document, string key, bool def)
    {
        return document.GetBoolOrDefault(key, def);
    }

    public static decimal? GetDecimal(this BsonDocument document, string key)
    {
        if (!document.TryGetValue(key, out var value))
        {
            return null;
        }

        if (value.IsDecimal)
        {
            return value;
        }

        return null;
    }

    public static decimal GetDecimalOrDefault(this BsonDocument document, string key, decimal def)
    {
        if (!document.TryGetValue(key, out var value))
        {
            return def;
        }

        if (value.IsDecimal)
        {
            return value;
        }

        return def;
    }

    public static Guid? GetGuid(this BsonDocument document, string key)
    {
        if (!document.TryGetValue(key, out var value))
        {
            return null;
        }

        if (value.IsGuid)
        {
            return value;
        }

        return null;
    }

    public static Guid GetGuidOrDefault(this BsonDocument document, string key, Guid def)
    {
        if (!document.TryGetValue(key, out var value))
        {
            return def;
        }

        if (value.IsGuid)
        {
            return value;
        }

        return def;
    }
}
