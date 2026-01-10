using System;
using Microsoft.Data.Sqlite;
using Nestor.Db.Models;

namespace Nestor.Db.Helpers;

public static class EventEntityExtension
{
    public static SqlQuery ToSqlQuery(this EventEntity e)
    {
        var tableName = e.GetTableName();

        return new(
            $"UPDATE {tableName} SET {e.EntityProperty} = @Value WHERE Id = @Id",
            new SqliteParameter("@Id", e.EntityId),
            new SqliteParameter(
                "@Value",
                e.EntityBooleanValue
                    ?? e.EntityByteArrayValue
                    ?? e.EntityByteValue
                    ?? e.EntityCharValue
                    ?? e.EntityDateOnlyValue
                    ?? e.EntityDateTimeOffsetValue
                    ?? e.EntityDateTimeValue
                    ?? e.EntityDecimalValue
                    ?? e.EntityDoubleValue
                    ?? e.EntityGuidValue
                    ?? e.EntityInt16Value
                    ?? e.EntityInt32Value
                    ?? e.EntityDateTimeOffsetValue
                    ?? e.EntityInt64Value
                    ?? e.EntitySByteValue
                    ?? e.EntitySingleValue
                    ?? e.EntityStringValue
                    ?? e.EntityTimeOnlyValue
                    ?? e.EntityTimeSpanValue
                    ?? e.EntityUInt16Value
                    ?? e.EntityUInt32Value
                    ?? e.EntityUInt64Value
                    ?? (object)DBNull.Value
            )
        );
    }

    public static string GetTableName(this EventEntity @event)
    {
        return $"{@event.EntityType.Substring(0, @event.EntityType.Length - 6)}s";
    }
}
