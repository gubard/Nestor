using Nestor.Db.Models;

[assembly: Ado(typeof(EventEntity), nameof(EventEntity.Id), true)]
[assembly: Ado(typeof(MigrationEntity), nameof(MigrationEntity.Id), false)]
[assembly: Ado(typeof(ObjectEntity), nameof(ObjectEntity.Key), false)]
[assembly: Ado(typeof(IdempotentEntity), nameof(IdempotentEntity.Id), false)]
[assembly: EditModel(typeof(EventEntity), nameof(EventEntity.Id))]
[assembly: EditModel(typeof(MigrationEntity), nameof(MigrationEntity.Id))]
[assembly: EditModel(typeof(ObjectEntity), nameof(ObjectEntity.Key))]
[assembly: EditModel(typeof(IdempotentEntity), nameof(IdempotentEntity.Id))]
