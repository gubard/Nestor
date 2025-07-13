using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nestor.Db.Sqlite;

public class EventEntityTypeConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.EntityType).HasMaxLength(255);
        builder.Property(e => e.EntityProperty).HasMaxLength(255);
        builder.Property(e => e.UserId).HasMaxLength(255);
    }
}