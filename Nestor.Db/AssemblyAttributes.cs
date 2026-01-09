using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(EventEntity), nameof(EventEntity.Id), true)]
[assembly: SqliteAdo(typeof(MigrationEntity), nameof(MigrationEntity.Id), false)]
