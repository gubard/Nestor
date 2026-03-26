using System;

namespace Nestor.Db.Models;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class EditModel : Attribute
{
    public EditModel(Type type, string idName)
    {
        Type = type;
        IdName = idName;
    }

    public Type Type { get; }
    public string IdName { get; }
}
