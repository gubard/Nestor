using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(EventEntity), nameof(EventEntity.Id), true)]
[assembly: SqliteAdo(typeof(MigrationEntity), nameof(MigrationEntity.Id), false)]
[assembly: SqliteAdo(typeof(ObjectEntity), nameof(ObjectEntity.Key), false)]
[assembly: SqliteAdo(typeof(IdempotentEntity), nameof(IdempotentEntity.Id), false)]
