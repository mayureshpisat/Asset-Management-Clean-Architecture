using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Reflection;

namespace Infrastructure.Persistence
{
    public class AssetDbContext : DbContext
    {
        public AssetDbContext(DbContextOptions<AssetDbContext> options) : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Signal> Signals { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<HierarchyVersion> HierarchyVersions { get; set; }

        public DbSet<AssetLog> AssetLogs { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<AssetInfo> AssetInfos { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
        }


    }
}
