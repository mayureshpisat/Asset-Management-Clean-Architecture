using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Username)
               .IsRequired()
               .HasMaxLength(32);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(32);

        builder.Property(u => u.Role)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(u => u.CreatedAtUtc);

        // Unique constraint
        builder.HasIndex(u => u.Username).IsUnique();

        // Seed admin user
        builder.HasData(new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@assetmanagement.in",
            Role = "Admin",
            PasswordHash = "AQAAAAIAAYagAAAAEIdgTPG+a4yi3RXmgJdIP+/vlZvopEnVlHnjJDJmVq/rGjZljrxpK1RoZ+iFAg0mhw==",
            CreatedAtUtc = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
