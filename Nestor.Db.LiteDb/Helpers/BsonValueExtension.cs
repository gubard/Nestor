using UltraLiteDB;

namespace Nestor.Db.LiteDb.Helpers;

public static class BsonValueExtension
{
    public static TEnum? ToEnumOrNull<TEnum>(this BsonValue value)
        where TEnum : struct, Enum
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToEnum<TEnum>();
    }

    public static TEnum ToEnum<TEnum>(this BsonValue value)
        where TEnum : struct, Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

        if (underlyingType == typeof(byte))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.ToByte());
        }

        if (underlyingType == typeof(sbyte))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.ToSByte());
        }

        if (underlyingType == typeof(short))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.ToInt16());
        }

        if (underlyingType == typeof(ushort))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.ToUInt16());
        }

        if (underlyingType == typeof(int))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.AsInt32);
        }

        if (underlyingType == typeof(uint))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.ToUInt32());
        }

        if (underlyingType == typeof(long))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value.AsInt64);
        }

        if (underlyingType == typeof(ulong))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), (ulong)value);
        }

        throw new NotSupportedException();
    }

    public static BsonValue ToBsonValue<TEnum>(this TEnum? value)
        where TEnum : struct, Enum
    {
        if (!value.HasValue)
        {
            return BsonValue.Null;
        }

        return value.Value.ToBsonValue();
    }

    public static BsonValue ToBsonValue<TEnum>(this TEnum value)
        where TEnum : struct, Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

        if (underlyingType == typeof(byte))
        {
            return Convert.ToByte(value).ToBsonValue();
        }

        if (underlyingType == typeof(sbyte))
        {
            return Convert.ToSByte(value);
        }

        if (underlyingType == typeof(short))
        {
            return Convert.ToInt16(value);
        }

        if (underlyingType == typeof(ushort))
        {
            return Convert.ToUInt16(value).ToBsonValue();
        }

        if (underlyingType == typeof(int))
        {
            return Convert.ToInt32(value);
        }

        if (underlyingType == typeof(uint))
        {
            return Convert.ToUInt32(value).ToBsonValue();
        }

        if (underlyingType == typeof(long))
        {
            return Convert.ToInt64(value);
        }

        if (underlyingType == typeof(ulong))
        {
            return Convert.ToUInt64(value);
        }

        throw new NotSupportedException();
    }

    public static BsonValue ToBsonValue(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcTicks;
    }

    public static BsonValue ToBsonValue(this DateTimeOffset? dateTimeOffset)
    {
        if (dateTimeOffset.HasValue)
        {
            return dateTimeOffset.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static DateTimeOffset ToDateTimeOffset(this BsonValue value)
    {
        return new DateTimeOffset(value.AsInt64, TimeSpan.Zero);
    }

    public static DateTimeOffset? ToDateTimeOffsetOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToDateTimeOffset();
    }

    public static BsonValue ToBsonValue(this byte? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this byte value)
    {
        return new[] { value };
    }

    public static byte ToByte(this BsonValue value)
    {
        return value.AsBinary[0];
    }

    public static byte? ToByteOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToByte();
    }

    public static BsonValue ToBsonValue(this ushort? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this ushort value)
    {
        return BsonValue.FromObject(value);
    }

    public static ushort ToUInt16(this BsonValue value)
    {
        return value.AsType<ushort>();
    }

    public static ushort? ToUInt16OrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToUInt16();
    }

    public static BsonValue ToBsonValue(this uint? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this uint value)
    {
        return BsonValue.FromObject(value);
    }

    public static uint ToUInt32(this BsonValue value)
    {
        return value.AsType<uint>();
    }

    public static uint? ToUInt32OrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToUInt32();
    }

    public static BsonValue ToBsonValue(this char? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this char value)
    {
        return BsonValue.FromObject(value);
    }

    public static char ToChar(this BsonValue value)
    {
        return value.AsType<char>();
    }

    public static char? ToCharOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToChar();
    }

    public static BsonValue ToBsonValue(this DateOnly? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this DateOnly value)
    {
        return value.DayNumber;
    }

    public static DateOnly ToDateOnly(this BsonValue value)
    {
        return DateOnly.MinValue.AddDays(value.AsInt32);
    }

    public static DateOnly? ToDateOnlyOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToDateOnly();
    }

    public static BsonValue ToBsonValue(this TimeOnly? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this TimeOnly value)
    {
        return value.Ticks;
    }

    public static TimeOnly ToTimeOnly(this BsonValue value)
    {
        return new TimeOnly(value.AsInt64);
    }

    public static TimeOnly? ToTimeOnlyOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToTimeOnly();
    }

    public static BsonValue ToBsonValue(this TimeSpan? value)
    {
        if (value.HasValue)
        {
            return value.Value.ToBsonValue();
        }

        return BsonValue.Null;
    }

    public static BsonValue ToBsonValue(this TimeSpan value)
    {
        return value.Ticks;
    }

    public static TimeSpan ToTimeSpan(this BsonValue value)
    {
        return new TimeSpan(value.AsInt64);
    }

    public static TimeSpan? ToTimeSpanOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToTimeSpan();
    }

    public static byte[] ToByteArray(this BsonValue value)
    {
        return value.AsType<byte[]>();
    }

    public static byte[]? ToByteArrayOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToByteArray();
    }

    public static sbyte ToSByte(this BsonValue value)
    {
        return value.AsType<sbyte>();
    }

    public static sbyte? ToSByteOrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToSByte();
    }

    public static short ToInt16(this BsonValue value)
    {
        return value.AsType<short>();
    }

    public static short? ToInt16OrNull(this BsonValue value)
    {
        if (value.IsNull)
        {
            return null;
        }

        return value.ToInt16();
    }
}
