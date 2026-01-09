namespace Nestor.Db.Models;

public struct UpdateProperty
{
    public readonly string PropertyName;
    public readonly object? Value;

    public UpdateProperty(string propertyName, object? value)
    {
        PropertyName = propertyName;
        Value = value;
    }
}
