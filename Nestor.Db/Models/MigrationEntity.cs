namespace Nestor.Db.Models;

public class MigrationEntity
{
    public long Id { get; set; }
    public string Sql { get; set; } = string.Empty;
}
