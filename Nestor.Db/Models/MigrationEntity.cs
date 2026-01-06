namespace Nestor.Db.Models;

public partial class MigrationEntity
{
    public int Id { get; set; }
    public string Sql { get; set; } = string.Empty;
}
