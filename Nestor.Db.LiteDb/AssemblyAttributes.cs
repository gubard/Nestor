using Nestor.Db.LiteDb.Models;
using Nestor.Db.Models;

[assembly: LiteDb(typeof(EventEntity), nameof(EventEntity.Id), true)]
[assembly: LiteDb(typeof(MigrationEntity), nameof(MigrationEntity.Id), false)]
[assembly: LiteDb(typeof(ObjectEntity), nameof(ObjectEntity.Key), false)]
[assembly: LiteDb(typeof(IdempotentEntity), nameof(IdempotentEntity.Id), false)]
