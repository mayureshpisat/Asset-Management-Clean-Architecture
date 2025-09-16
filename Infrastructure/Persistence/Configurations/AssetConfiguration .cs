using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        // Primary key
        builder.HasKey(a => a.Id);

        // Name property with validation
        builder.Property(a => a.Name)
               .IsRequired()
               .HasMaxLength(30);

        // Self-referencing relationship
        builder.HasOne(a => a.Parent)          // each asset has one parent
               .WithMany(a => a.Children)      // parent can have many children
               .HasForeignKey(a => a.ParentId) // FK is ParentId
               .OnDelete(DeleteBehavior.ClientCascade); // prevent cascade delete from nuking whole tree
    }
}
