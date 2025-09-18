using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class AssetInfoConfiguration : IEntityTypeConfiguration<AssetInfo>
    {
        public void Configure(EntityTypeBuilder<AssetInfo> builder)
        {
            List<AssetInfo> assets = new List<AssetInfo>();
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Temperature);

            builder.Property(a => a.Power);

            builder.HasOne(a => a.Asset).WithMany()
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);


        }

        

    }
}
