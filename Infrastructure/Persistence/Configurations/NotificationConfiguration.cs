using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Primary Key
        builder.HasKey(n => n.Id);

        // Relationships
        builder.HasOne(n => n.User)
               .WithMany() // if User has a Notifications collection, replace with .WithMany(u => u.Notifications)
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Properties
        builder.Property(n => n.Type)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(n => n.Message)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(n => n.SenderName)
               .HasMaxLength(100);

        builder.Property(n => n.IsRead);

        builder.Property(n => n.CreatedAt);
    }
}
