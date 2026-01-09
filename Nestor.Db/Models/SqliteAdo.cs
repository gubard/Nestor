using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SqliteAdo : Attribute
{
    public SqliteAdo(Type type, string idName)
    {
        Type = type;
        IdName = idName;
    }

    public Type Type { get; }
    public string IdName { get; }
}
