using Nestor.Db.Models;

[assembly: Ado(typeof(EventEntity), nameof(EventEntity.Id), true)]
[assembly: Ado(typeof(MigrationEntity), nameof(MigrationEntity.Id), false)]
[assembly: Ado(typeof(ObjectEntity), nameof(ObjectEntity.Key), false)]
[assembly: Ado(typeof(IdempotentEntity), nameof(IdempotentEntity.Id), false)]
