using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nestor.Db.Helpers;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public sealed class EventEntityTypeConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd().SetComparerStruct();
        builder.Property(e => e.EntityType).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.EntityProperty).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.UserId).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.EntityTimeOnlyValue).SetComparerNullStruct();
        builder.Property(e => e.EntityId).SetComparerStruct();
        builder.Property(e => e.EntityDecimalValue).SetComparerNullStruct();
        builder.Property(e => e.EntityDateTimeValue).SetComparerNullStruct();
        builder.Property(e => e.CreatedAt).SetComparerStruct();
        builder.Property(e => e.EntityDateTimeOffsetValue).SetComparerNullStruct();
        builder.Property(e => e.EntityDateOnlyValue).SetComparerNullStruct();
        builder.Property(e => e.EntityBooleanValue).SetComparerNullStruct();
        builder.Property(e => e.EntityByteValue).SetComparerNullStruct();
        builder.Property(e => e.EntityUInt16Value).SetComparerNullStruct();
        builder.Property(e => e.EntityUInt32Value).SetComparerNullStruct();
        builder.Property(e => e.EntityUInt64Value).SetComparerNullStruct();
        builder.Property(e => e.EntitySByteValue).SetComparerNullStruct();
        builder.Property(e => e.EntityInt16Value).SetComparerNullStruct();
        builder.Property(e => e.EntityInt32Value).SetComparerNullStruct();
        builder.Property(e => e.EntityInt64Value).SetComparerNullStruct();
        builder.Property(e => e.EntitySingleValue).SetComparerNullStruct();
        builder.Property(e => e.EntityDoubleValue).SetComparerNullStruct();
        builder.Property(e => e.EntityCharValue).SetComparerNullStruct();
        builder.Property(e => e.EntityByteArrayValue).SetComparerNullClass();
        builder.Property(e => e.EntityStringValue).SetComparerNullClass();
        builder.Property(e => e.EntityGuidValue).SetComparerNullStruct();
        builder.Property(e => e.EntityDateTimeValue).SetComparerNullStruct();
        builder.Property(e => e.EntityDateTimeOffsetValue).SetComparerNullStruct();
        builder.Property(e => e.EntityDateOnlyValue).SetComparerNullStruct();
        builder.Property(e => e.EntityTimeOnlyValue).SetComparerNullStruct();
        builder.Property(e => e.EntityTimeSpanValue).SetComparerNullStruct();
        builder.Property(e => e.TransactionId).SetComparerStruct();
    }
}

public sealed class MigrationEntityTypeConfiguration : IEntityTypeConfiguration<MigrationEntity>
{
    public void Configure(EntityTypeBuilder<MigrationEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever().SetComparerStruct();
        builder.Property(e => e.Sql).SetComparerClass();
    }
}
