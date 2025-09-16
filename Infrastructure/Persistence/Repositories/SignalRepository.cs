using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories
{
    public class SignalRepository : ISignalRepository
    {

        private readonly AssetDbContext _dbContext;

        public SignalRepository(AssetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task<Asset> GetAssetWithSignalsAsync(int assetId)
        {
            var asset = await _dbContext.Assets.Include(a => a.Signals).FirstOrDefaultAsync(a => a.Id == assetId);
            return asset;
        }

        public async Task<Asset> GetAssetAsync(int assetId)
        {
            var asset = await _dbContext.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
            return asset;
        }

        public async Task RemoveSignalAsync(Signal signal)
        {
            _dbContext.Signals.Remove(signal);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Asset> GetRootWithChildrenAsync()
        {
            var root = await _dbContext.Assets.FirstOrDefaultAsync(a => a.ParentId == null);
            LoadChildren(root);
            return root;
        }

        private void LoadChildren(Asset parent)
        {
            _dbContext.Entry(parent).Collection(p => p.Children).Load();
            _dbContext.Entry(parent).Collection(p => p.Signals).Load();
            foreach (var child in parent.Children)
            {
                LoadChildren(child);
            }
        }


    }
}
