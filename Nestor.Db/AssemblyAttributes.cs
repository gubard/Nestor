using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(EventEntity), nameof(EventEntity.Id))]
[assembly: SqliteAdo(typeof(MigrationEntity), nameof(MigrationEntity.Id))]
