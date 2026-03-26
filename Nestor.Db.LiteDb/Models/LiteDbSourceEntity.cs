namespace Nestor.Db.LiteDb.Models;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class LiteDbSourceEntity : Attribute
{
    public LiteDbSourceEntity(Type type, string idName)
    {
        Type = type;
        IdName = idName;
    }

    public Type Type { get; }
    public string IdName { get; }
}
