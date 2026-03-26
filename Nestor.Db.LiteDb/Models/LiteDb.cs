namespace Nestor.Db.LiteDb.Models;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class LiteDb : Attribute
{
    public LiteDb(Type type, string idName, bool isAutoIncrementId)
    {
        Type = type;
        IdName = idName;
        IsAutoIncrementId = isAutoIncrementId;
    }

    public Type Type { get; }
    public string IdName { get; }
    public bool IsAutoIncrementId { get; set; }
}
