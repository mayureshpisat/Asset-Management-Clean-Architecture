using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeders
{
    public static class AssetInfoSeeder
    {

        public static async Task SeedInfoAsync(AssetDbContext dbContext)
        {
            if (!dbContext.AssetInfos.Any())
            {
                Console.WriteLine($"AssetInfoSeeder: Assetinfo not found");
                List<AssetInfo> assetInfos = new List<AssetInfo>();
                Random random = new Random();
                for(int i =1; i<=25; i++)
                {
                    if(dbContext.Assets.Any(a=>a.Id == i))
                    {
                        Console.WriteLine($"AssetInfoSeeder: Asset with Id {i} found making 1000 for Asset {i}");

                        for (int j = 1; j <= 10000; j++)
                        {

                            assetInfos.Add(new AssetInfo
                            {
                                AssetId = i,
                                Temperature = random.Next(1,501),
                                Power = random.Next(1,501)

                            });
                        }
                        Console.WriteLine($"AssetInfoSeeder: Done for asset with Id {i}");



                    }
                }

                dbContext.AssetInfos.AddRange(assetInfos);
                await dbContext.SaveChangesAsync();

            }
        }
    }
}
