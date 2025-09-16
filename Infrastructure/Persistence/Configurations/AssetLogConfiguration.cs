using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AssetLogConfiguration : IEntityTypeConfiguration<AssetLog>
{
    public void Configure(EntityTypeBuilder<AssetLog> builder)
    {
        // Primary Key
        builder.HasKey(al => al.Id);

        // Relationships
        builder.HasOne(al => al.User)
               .WithMany()
               .HasForeignKey(al => al.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Properties
        builder.Property(al => al.Asset)
               .HasMaxLength(100);

        builder.Property(al => al.Signal)
               .HasMaxLength(100);

        builder.Property(al => al.Action)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(al => al.LogTime);
               
    }
}
