namespace Nestor.Db.Models;

[InsertQuery]
public partial class MigrationEntity
{
    public int Id { get; set; }
    public string Sql { get; set; } = string.Empty;
}
