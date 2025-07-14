using System;

namespace Nestor.Db;

[AttributeUsage(AttributeTargets.Class)]
public class SourceEntity : Attribute
{
    public SourceEntity(string idPropertyName)
    {
        IdPropertyName = idPropertyName;
    }

    public string IdPropertyName { get; }
}