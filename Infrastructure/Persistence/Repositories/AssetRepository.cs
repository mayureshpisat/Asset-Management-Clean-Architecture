using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly AssetDbContext _dbContext;
        public AssetRepository(AssetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Asset> GetRootWithChildrenAsync()
        {
            var root = await _dbContext.Assets.FirstOrDefaultAsync(a => a.ParentId == null);
            await LoadChildrenAsync(root);
            return root;
        }
        private async Task LoadChildrenAsync(Asset parent)
        {
            await _dbContext.Entry(parent).Collection(p => p.Children).LoadAsync();
            await _dbContext.Entry(parent).Collection(p => p.Signals).LoadAsync();
            foreach (var child in parent.Children)
            {
                await LoadChildrenAsync(child);
            }
        }

        public async Task AddAsync(Asset node)
        {
            await _dbContext.Assets.AddAsync(node);

        }
        public async Task<Asset> GetAssetByIdAsync(int assetId)
        {
            var asset = await _dbContext.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
            return asset;
        }

        public async Task<Asset> GetAssetByNameAsync(string assetName)
        {
            var asset = await _dbContext.Assets.FirstOrDefaultAsync(a=>a.Name == assetName);
            return asset;
        }

        public async Task<Asset> GetAssetWithChildrenAsync(int assetId)
        {
            var asset = await _dbContext.Assets
                .Include(a => a.Children)
                .FirstOrDefaultAsync(a => a.Id == assetId);

            return asset;
        }

        public async Task<bool> CheckAssetByNameAsync(string assetName)
        {
            return await _dbContext.Assets.AnyAsync(a => a.Name == assetName);
        }

        public async Task<Asset> GetRootAssetAsync()
        {
            return await _dbContext.Assets.FirstOrDefaultAsync(a => a.ParentId == null);
        }

        public async Task LoadAssetWithChildrenAsync(Asset parent)
        {
            await _dbContext.Entry(parent).Collection(a => a.Children).LoadAsync();
        }

        public void RemoveAssetAsync(Asset asset)
        {
            _dbContext.Assets.Remove(asset);
        }

        public async Task TruncateAssetsAsync()
        {
            // TRUNCATE can't be used if table has FKs → so you wrap in try/catch in service
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Assets");
        }

        public async Task DeleteAllAssetsAsync()
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Assets");
        }

        public async Task ReseedAssetsIdentityAsync()
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Assets', RESEED, 0)");
        }





    }
}
