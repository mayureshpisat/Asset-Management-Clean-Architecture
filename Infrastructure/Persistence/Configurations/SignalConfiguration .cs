using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SignalConfiguration : IEntityTypeConfiguration<Signal>
{
    public void Configure(EntityTypeBuilder<Signal> builder)
    {
        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(s => s.ValueType)
               .IsRequired();

        builder.Property(s => s.Description)
               .HasMaxLength(250);

        // Relationship
        builder.HasOne(s => s.Asset)           // Signal belongs to one Asset
               .WithMany(a => a.Signals)       // Asset has many Signals
               .HasForeignKey(s => s.AssetId)  // FK is AssetId
               .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: Signal names must be unique per Asset
        builder.HasIndex(s => new { s.AssetId, s.Name })
               .IsUnique();
    }
}
