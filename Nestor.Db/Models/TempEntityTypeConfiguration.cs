using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nestor.Db.Models;

public class TempEntityTypeConfiguration : IEntityTypeConfiguration<TempEntity>
{

    public void Configure(EntityTypeBuilder<TempEntity> builder)
    {
        builder.HasKey(e => e.EntityId);
    }
}