using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class SqliteAdo : Attribute
{
    public SqliteAdo(Type type, string idName, bool isAutoIncrementId)
    {
        Type = type;
        IdName = idName;
        IsAutoIncrementId = isAutoIncrementId;
    }

    public Type Type { get; }
    public string IdName { get; }
    public bool IsAutoIncrementId { get; set; }
}
