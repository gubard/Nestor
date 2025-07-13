using System;

namespace Nestor.Db;

public class SourceEntity : Attribute
{
    public SourceEntity(string idPropertyName)
    {
        IdPropertyName = idPropertyName;
    }

    public string IdPropertyName { get; }
}