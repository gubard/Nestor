using Nestor.Db;

namespace Nestor.Shared;

[SourceEntity(nameof(Id))]
public partial class Example
{
    public Guid Id { get; set; }
    public bool ExampleBooleanValue { get; set; }
    public byte ExampleByteValue { get; set; }
    public ushort ExampleUInt16Value { get; set; }
    public uint ExampleUInt32Value { get; set; }
    public ulong ExampleUInt64Value { get; set; }
    public sbyte ExampleSByteValue { get; set; }
    public short ExampleInt16Value { get; set; }
    public int ExampleInt32Value { get; set; }
    public long ExampleInt64Value { get; set; }
    public float ExampleSingleValue { get; set; }
    public double ExampleDoubleValue { get; set; }
    public decimal ExampleDecimalValue { get; set; }
    public char ExampleCharValue { get; set; }
    public byte[] ExampleByteArrayValue { get; set; } = [];
    public string ExampleStringValue { get; set; } = string.Empty;
    public Guid ExampleGuidValue { get; set; }
    public DateTime ExampleDateTimeValue { get; set; }
    public DateTimeOffset ExampleDateTimeOffsetValue { get; set; }
    public DateOnly ExampleDateOnlyValue { get; set; }
    public TimeOnly ExampleTimeOnlyValue { get; set; }
    public TimeSpan ExampleTimeSpanValue { get; set; }
    public ExampleEnum ExampleEnumValue { get; set; }
}
