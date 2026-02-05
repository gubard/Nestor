using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Nestor.Db.Helpers;

public static class EventEntityExtension
{
    public static SqlQuery ToSqlQuery(this EventEntity e, DbSession session)
    {
        var tableName = e.GetTableName();

        if (e.EntityProperty == "__IS_DELETED__" && e.EntityBooleanValue == true)
        {
            if (e.EntityBooleanValue == true)
            {
                return new(
                    $"DELETE FROM {tableName} WHERE Id = @Id",
                    session.CreateParameter("@Id", e.EntityId)
                );
            }

            return InsertHelper.CreateDefaultInsert(e.EntityType, e.EntityId, session);
        }

        return new(
            $"UPDATE {tableName} SET {e.EntityProperty} = @Value WHERE Id = @Id",
            session.CreateParameter("@Id", e.EntityId),
            session.CreateParameter(
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
                    ?? (object?)e.EntityUInt64Value
            )
        );
    }

    public static string GetTableName(this EventEntity @event)
    {
        return $"{@event.EntityType.Substring(0, @event.EntityType.Length - 6)}s";
    }
}
