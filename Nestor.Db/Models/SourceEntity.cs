using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Class)]
public class SourceEntity : Attribute
{
    public SourceEntity(string idPropertyName, Type dbContextType)
    {
        IdPropertyName = idPropertyName;
        DbContextType = dbContextType;
    }

    public string IdPropertyName { get; }
    public Type DbContextType { get; }
}
