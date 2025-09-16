using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISignalRepository
    {
        Task SaveChangesAsync();

        Task<Asset> GetAssetWithSignalsAsync(int assetId);

        Task<Asset> GetAssetAsync(int assetId);

        Task RemoveSignalAsync(Signal signal);

        Task<Asset> GetRootWithChildrenAsync();
    }
}
