using Nestor.Db.Models;
using UltraLiteDB;

namespace Nestor.Db.LiteDb.Helpers;

public static class EventEntityExtension
{
    public static string GetEntityCollectionName(this EventEntity entity)
    {
        return entity.EntityType;
    }

    public static BsonValue GetBsonValue(this EventEntity entity)
    {
        if (entity.EntityBooleanValue.HasValue)
        {
            return entity.EntityBooleanValue;
        }

        if (entity.EntityByteValue.HasValue)
        {
            return entity.EntityByteValue.ToBsonValue();
        }

        if (entity.EntityUInt16Value.HasValue)
        {
            return entity.EntityUInt16Value.ToBsonValue();
        }

        if (entity.EntityUInt32Value.HasValue)
        {
            return entity.EntityUInt32Value.ToBsonValue();
        }

        if (entity.EntityUInt64Value.HasValue)
        {
            return entity.EntityUInt64Value;
        }

        if (entity.EntitySByteValue.HasValue)
        {
            return entity.EntitySByteValue;
        }

        if (entity.EntityInt16Value.HasValue)
        {
            return entity.EntityInt16Value;
        }

        if (entity.EntityInt32Value.HasValue)
        {
            return entity.EntityInt32Value;
        }

        if (entity.EntityInt64Value.HasValue)
        {
            return entity.EntityInt64Value;
        }

        if (entity.EntitySingleValue.HasValue)
        {
            return entity.EntitySingleValue;
        }

        if (entity.EntityDoubleValue.HasValue)
        {
            return entity.EntityDoubleValue;
        }

        if (entity.EntityDecimalValue.HasValue)
        {
            return entity.EntityDecimalValue;
        }

        if (entity.EntityCharValue.HasValue)
        {
            return entity.EntityCharValue.ToBsonValue();
        }

        if (entity.EntityByteArrayValue is null)
        {
            return entity.EntityByteArrayValue;
        }

        if (entity.EntityStringValue is null)
        {
            return entity.EntityStringValue;
        }

        if (entity.EntityGuidValue.HasValue)
        {
            return entity.EntityGuidValue;
        }

        if (entity.EntityDateTimeValue.HasValue)
        {
            return entity.EntityDateTimeValue;
        }

        if (entity.EntityDateTimeOffsetValue.HasValue)
        {
            return entity.EntityDateTimeOffsetValue.ToBsonValue();
        }

        if (entity.EntityDateOnlyValue.HasValue)
        {
            return entity.EntityDateOnlyValue.ToBsonValue();
        }

        if (entity.EntityTimeOnlyValue.HasValue)
        {
            return entity.EntityTimeOnlyValue.ToBsonValue();
        }

        if (entity.EntityTimeSpanValue.HasValue)
        {
            return entity.EntityTimeSpanValue.ToBsonValue();
        }

        return BsonValue.Null;
    }
}
