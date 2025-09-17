using Domain.Entities;


namespace Application.Interfaces
{

    public interface IAssetHierarchyService
    {
        Task<Asset> GetHierarchy();
        Task<bool> AddNode(int parentId, Asset newNode);
        Task<bool> AddToRoot(string assetName);

        Task<bool> RemoveNode(int nodeId);

        Task<bool> UpdateNode(int nodeId, string newName);

        Task ReorderNode(int assetId, int parentId);
        bool CheckDuplicated(Asset Node);
        int TotalAsset(Asset Node);
        Task ReplaceTree(Asset newRoot);
        Task<int> MergeTree(Asset AddtionalNode);
        Task<int> TreeLength(Asset Node);
    }
}