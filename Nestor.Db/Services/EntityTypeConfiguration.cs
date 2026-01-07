using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nestor.Db.Models;

namespace Nestor.Db.Services;

public sealed class EventEntityTypeConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.EntityType).HasMaxLength(255);
        builder.Property(e => e.EntityProperty).HasMaxLength(255);
        builder.Property(e => e.UserId).HasMaxLength(255);

        builder
            .Property(e => e.CreatedAt)
            .Metadata.SetValueComparer(
                new ValueComparer<DateTimeOffset>(
                    (c1, c2) => c1 == c2,
                    c => c.GetHashCode(),
                    c => c
                )
            );

        builder
            .Property(e => e.EntityDateTimeOffsetValue)
            .Metadata.SetValueComparer(
                new ValueComparer<DateTimeOffset?>(
                    (c1, c2) => c1 == c2,
                    c => c.GetHashCode(),
                    c => c
                )
            );
    }
}

public sealed class MigrationEntityTypeConfiguration : IEntityTypeConfiguration<MigrationEntity>
{
    public void Configure(EntityTypeBuilder<MigrationEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
    }
}
