using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAssetRepository
    {
        Task<Asset> GetRootWithChildrenAsync();

        Task AddAsync(Asset node);

        Task SaveChangesAsync();

        Task<Asset> GetAssetByIdAsync(int assetId);

        Task<Asset> GetAssetByNameAsync(string assetName);

        Task<Asset> GetAssetWithChildrenAsync(int assetId);

        Task<bool> CheckAssetByNameAsync(string assetName);

        Task<Asset> GetRootAssetAsync();

        Task LoadAssetWithChildrenAsync(Asset parent);

        void RemoveAssetAsync(Asset asset);

        Task TruncateAssetsAsync();
        Task DeleteAllAssetsAsync();
        Task ReseedAssetsIdentityAsync();
    }

}
