using Microsoft.Data.Sqlite;
using Nestor.Db.Models;

namespace Nestor.Db.Sqlite.Helpers;

public static class Mapper
{
    public static SqliteParameter[] ToSqliteParameters(this MigrationEntity entity)
    {
        return [new("@Id", entity.Id), new("@Sql", entity.Sql)];
    }

    public static SqliteParameter[] ToSqliteParameters(this EventEntity entity)
    {
        return
        [
            new("@Id", entity.Id),
            new("@EntityId", entity.EntityId),
            new("@EntityType", entity.EntityType),
            new("@EntityProperty", entity.EntityProperty),
            new("@UserId", entity.UserId),
            new("@CreatedAt", entity.CreatedAt),
            new("@EntityBooleanValue", (object?)entity.EntityBooleanValue ?? DBNull.Value),
            new("@EntityByteValue", (object?)entity.EntityByteValue ?? DBNull.Value),
            new("@EntityUInt16Value", (object?)entity.EntityUInt16Value ?? DBNull.Value),
            new("@EntityUInt32Value", (object?)entity.EntityUInt32Value ?? DBNull.Value),
            new("@EntityUInt64Value", (object?)entity.EntityUInt64Value ?? DBNull.Value),
            new("@EntitySByteValue", (object?)entity.EntitySByteValue ?? DBNull.Value),
            new("@EntityInt16Value", (object?)entity.EntityInt16Value ?? DBNull.Value),
            new("@EntityInt32Value", (object?)entity.EntityInt32Value ?? DBNull.Value),
            new("@EntityInt64Value", (object?)entity.EntityInt64Value ?? DBNull.Value),
            new("@EntitySingleValue", (object?)entity.EntitySingleValue ?? DBNull.Value),
            new("@EntityDoubleValue", (object?)entity.EntityDoubleValue ?? DBNull.Value),
            new("@EntityDecimalValue", (object?)entity.EntityDecimalValue ?? DBNull.Value),
            new("@EntityCharValue", (object?)entity.EntityCharValue ?? DBNull.Value),
            new("@EntityByteArrayValue", (object?)entity.EntityByteArrayValue ?? DBNull.Value),
            new("@EntityStringValue", (object?)entity.EntityStringValue ?? DBNull.Value),
            new("@EntityGuidValue", (object?)entity.EntityGuidValue ?? DBNull.Value),
            new("@EntityDateTimeValue", (object?)entity.EntityDateTimeValue ?? DBNull.Value),
            new(
                "@EntityDateTimeOffsetValue",
                (object?)entity.EntityDateTimeOffsetValue ?? DBNull.Value
            ),
            new("@EntityDateOnlyValue", (object?)entity.EntityDateOnlyValue ?? DBNull.Value),
            new("@EntityTimeOnlyValue", (object?)entity.EntityTimeOnlyValue ?? DBNull.Value),
            new("@EntityTimeSpanValue", (object?)entity.EntityTimeSpanValue ?? DBNull.Value),
            new("@IsLast", entity.IsLast),
        ];
    }
}
