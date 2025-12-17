using System;

namespace Nestor.Db.Models;

public class EventEntity
{
    public long Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityProperty { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public bool? EntityBooleanValue { get; set; }
    public byte? EntityByteValue { get; set; }
    public ushort? EntityUInt16Value { get; set; }
    public uint? EntityUInt32Value { get; set; }
    public ulong? EntityUInt64Value { get; set; }
    public sbyte? EntitySByteValue { get; set; }
    public short? EntityInt16Value { get; set; }
    public int? EntityInt32Value { get; set; }
    public long? EntityInt64Value { get; set; }
    public float? EntitySingleValue { get; set; }
    public double? EntityDoubleValue { get; set; }
    public decimal? EntityDecimalValue { get; set; }
    public char? EntityCharValue { get; set; }
    public byte[]? EntityByteArrayValue { get; set; }
    public string? EntityStringValue { get; set; }
    public Guid? EntityGuidValue { get; set; }
    public DateTime? EntityDateTimeValue { get; set; }
    public DateTimeOffset? EntityDateTimeOffsetValue { get; set; }
    public DateOnly? EntityDateOnlyValue { get; set; }
    public TimeOnly? EntityTimeOnlyValue { get; set; }
    public TimeSpan? EntityTimeSpanValue { get; set; }
    public bool IsLast { get; set; }
}
