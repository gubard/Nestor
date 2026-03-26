using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class AdoSourceEntity : Attribute
{
    public AdoSourceEntity(Type type, string idName)
    {
        Type = type;
        IdName = idName;
    }

    public Type Type { get; }
    public string IdName { get; }
}
