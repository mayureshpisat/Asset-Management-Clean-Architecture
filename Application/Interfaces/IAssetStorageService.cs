using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAssetStorageService
    {
        Asset LoadTree();
        void SaveTree(Asset root, string? action = null);

        string GetVersionedFileName();
        Asset ParseTree(string content); //content here is the new json file that user is uploading converted to a string.

    }
}


