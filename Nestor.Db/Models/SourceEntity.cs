using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Class)]
public class SourceEntity : Attribute
{
    public SourceEntity(string idPropertyName)
    {
        IdPropertyName = idPropertyName;
    }

    public string IdPropertyName { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class InsertQuery : Attribute;
