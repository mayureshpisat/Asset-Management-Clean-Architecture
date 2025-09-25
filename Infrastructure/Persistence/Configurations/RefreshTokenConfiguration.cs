using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);

            builder.Property(r => r.Token).HasMaxLength(200).IsRequired();

            builder.Property(r=>r.ExpiresAt).IsRequired();

            builder.Property(r => r.IsRevoked).HasDefaultValue(false);


        }
    }
}
