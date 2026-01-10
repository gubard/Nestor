namespace Nestor.Db.Models;

public readonly struct DbServiceOptions
{
    public readonly bool IsUseEvents;

    public DbServiceOptions(bool isUseEvents)
    {
        IsUseEvents = isUseEvents;
    }
}
