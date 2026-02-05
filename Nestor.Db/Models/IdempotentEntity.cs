using System;

namespace Nestor.Db.Models;

public sealed class IdempotentEntity
{
    public Guid Id { get; set; }
    public byte[] Data { get; set; } = [];
    public string DataType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
