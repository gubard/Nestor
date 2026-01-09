using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Assembly)]
public class SourceEntity : Attribute
{
    public SourceEntity(Type type, string idName)
    {
        Type = type;
        IdName = idName;
    }

    public Type Type { get; }
    public string IdName { get; }
}
