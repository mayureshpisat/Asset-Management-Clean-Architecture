using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeders
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AssetDbContext dbContext)
        {
            await AssetInfoSeeder.SeedInfoAsync(dbContext);
        }
    }
}
