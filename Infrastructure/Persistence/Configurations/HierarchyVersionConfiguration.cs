using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class HierarchyVersionConfiguration : IEntityTypeConfiguration<HierarchyVersion>
{
    public void Configure(EntityTypeBuilder<HierarchyVersion> builder)
    {
        // Primary Key
        builder.HasKey(h => h.Id);

        // Properties
        builder.Property(h => h.EditedTime);

        builder.Property(h => h.SnapshotJson)
               .IsRequired();

        builder.Property(h => h.Action)
               .IsRequired()
               .HasMaxLength(100);
    }
}
