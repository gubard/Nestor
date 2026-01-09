using Gaia.Helpers;
using Microsoft.Data.Sqlite;

namespace Nestor.Db.Helpers;

public static class ObjectExtension
{
    public static SqliteParameter[] ToSqliteParameters<T>(this T[] items, string parameterName)
    {
        var result = new SqliteParameter[items.Length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = new($"@{parameterName}{i}", items[i]);
        }

        return result;
    }

    public static string ToParameterNames<T>(this T[] items, string parameterName)
    {
        var result = new string[items.Length];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = $"@{parameterName}{i}";
        }

        return result.JoinString(", ");
    }
}
