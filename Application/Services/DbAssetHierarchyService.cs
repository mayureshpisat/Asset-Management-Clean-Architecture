using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Services
{
    public enum AssetAction
    {
        AddAsset = 1,
        UpdateAsset = 2,
        DeleteAsset = 3,
        ReplaceHierarchy = 4,
        MergeHierarchy = 5
    }



    public class DbAssetHierarchyService : IAssetHierarchyService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetStorageService _storage;
        public static List<Asset> assetsAdded = new List<Asset>();
        public readonly IAssetLogService _logService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        private readonly INotificationStoreService _notificationStoreService;
        public static string notificationType = "Asset";

        public DbAssetHierarchyService(INotificationService notificationService, IAssetStorageService storage, IAssetLogService logService,  IHttpContextAccessor httpContextAccessor, IAssetRepository assetRepository, INotificationStoreService notificationStoreService)
        {
            _storage = storage;
            _logService = logService;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _assetRepository = assetRepository;
            _notificationStoreService = notificationStoreService;
        }


        private string? GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        }

        private string? GetCurrentUserID()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        }
        private string SerializeJson(Asset asset)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };


            string json = JsonConvert.SerializeObject(asset, settings);

            return json;
        }
        public async Task<Asset> GetHierarchy()
        {
            var root = await _assetRepository.GetRootWithChildrenAsync();
            if (root == null)
            {
                root = new Asset { Name = "Root" };
                await _assetRepository.AddAsync(root);
                await _assetRepository.SaveChangesAsync();
                return root;
            }
            return root;
           
        }


        private async Task SaveHierarchyVersion(string? action = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                action = "None";

            //in memory objects reflect db state
            //saving in file for downloading and tracking purpose.
            //recursivley load children in root to represent deep hierarchy
            var root = await _assetRepository.GetRootWithChildrenAsync();
            _storage.SaveTree(root, action);
            Console.WriteLine("VERSION SAVED");

        }


        public async Task<bool> AddNode(int parentId, Asset newNode)
        {
            var parent = await _assetRepository.GetAssetByIdAsync(parentId);
            Console.WriteLine($"FROM ADD NODE parentID = {parent.Id}, parentName = {parent.Name}");
            if (parent == null) return false;


            // Prevent duplicate Name 
            //if (parent.Children.Any(c => c.Name == newNode.Name)) return false;
            if (await _assetRepository.GetAssetByNameAsync(newNode.Name) != null)
            {

                return false;
            }

            string action = "Asset Add";
            parent.Children.Add(newNode);
            await _assetRepository.SaveChangesAsync();

            //save hierarchy
            await SaveHierarchyVersion(action);
            await _logService.Log(action, asset: newNode.Name);


            //notification
            var currentUserId = GetCurrentUserID();
            var currentUser = GetCurrentUser();

            AssetNotificationDTO notification = new AssetNotificationDTO
            {
                User = currentUserId,
                Name = newNode.Name,
                ParentName = parent.Name,
                Type = "AssetAdded"
            };
            await _notificationService.BroadcastToAdminsAndViewers(currentUserId, notification, notificationType);
            return true;
        }
        public async Task ReorderNode(int assetId, int parentId)
        {
            string action = "Reorder Asset";
            try
            {
                var parent = await _assetRepository.GetAssetByIdAsync(parentId);
                var asset = await _assetRepository.GetAssetByIdAsync(assetId);

                if (parent == null)
                    throw new Exception("Target parent asset not found");
                if (asset == null)
                    throw new Exception("Asset to move not found");

                // Check if trying to move asset under itself
                if (assetId == parentId)
                    throw new Exception("Cannot move asset under itself");

                // Check if trying to move asset under its descendant (would create circular reference)
                // We need to check if the NEW PARENT is a descendant of the ASSET BEING MOVED
                if (await IsDescendant(assetId, parentId))
                    throw new Exception("Cannot move asset under its descendant - this would create a circular reference");

                // Check if asset is already under this parent
                if (asset.ParentId == parentId)
                    throw new Exception("Asset is already under the specified parent");

                asset.ParentId = parentId;
                asset.Parent = parent;
                await _assetRepository.SaveChangesAsync();
                //save hierarchy
                await SaveHierarchyVersion(action);


                //notification
                var currentUserId = GetCurrentUserID();
                var currentUser = GetCurrentUser();

                AssetNotificationDTO notification = new AssetNotificationDTO
                {
                    User = currentUserId,
                    Type = "AssetReorder"
                };
                await _notificationService.BroadcastToAdminsAndViewers(currentUserId, notification, notificationType);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Invalid asset operation");
            }
        }

        // Helper method to check if targetId is a descendant of ancestorId
        private async Task<bool> IsDescendant(int ancestorId, int targetId)
        {
            var ancestorAsset = await _assetRepository.GetAssetWithChildrenAsync(ancestorId);

            if (ancestorAsset == null) return false;

            return await CheckDescendantRecursive(ancestorAsset, targetId);
        }

        private async Task<bool> CheckDescendantRecursive(Asset currentAsset, int targetId)
        {
            if (currentAsset.Children == null || !currentAsset.Children.Any())
                return false;

            foreach (var child in currentAsset.Children)
            {
                if (child.Id == targetId)
                    return true;

                // Load children for recursive check
                var childWithChildren = await _assetRepository.GetAssetWithChildrenAsync(child.Id);

                if (childWithChildren != null && await CheckDescendantRecursive(childWithChildren, targetId))
                    return true;
            }

            return false;
        }
        public async Task<bool> AddToRoot(string assetName)
        {
            bool isPresent = await _assetRepository.CheckAssetByNameAsync(assetName);
            if (isPresent)
            {
                return false;
            }
            var root = await _assetRepository.GetRootAssetAsync();
            var asset = new Asset
            {
                Name = assetName,
                Children = new List<Asset>(),
                Signals = new List<Signal>()

            };
            string action = "Asset Add";
            root.Children.Add(asset);
            await _assetRepository.SaveChangesAsync();

            //save version
            await SaveHierarchyVersion(action);
            await _logService.Log(action, assetName);


            //notification
            var currentUserId = GetCurrentUserID();
            var currentUser = GetCurrentUser();
            AssetNotificationDTO notification = new AssetNotificationDTO
            {
                User = currentUserId,
                Name = assetName,
                Type = "AssetAdded"
            };
            await _notificationService.BroadcastToAdminsAndViewers(currentUserId, notification, notificationType);

            string notificationMessage = $"{currentUser} added asset {assetName} to the root";
            await _notificationStoreService.SaveNotificationsForOfflineUsers(notificationType, notificationMessage, int.Parse(currentUserId), currentUser);

            return true;
        }

        public async Task<bool> RemoveNode(int nodeId)
        {

            var node = await _assetRepository.GetAssetByIdAsync(nodeId);
            if (node.ParentId == null) return false; //disallow deleting root node.
            if (node == null) return false;

            string name = node.Name; //for notification purpose
            await DeleteRecursively(node);   // handles children + node itself
            await _assetRepository.SaveChangesAsync();
            string action = "Delete Asset";

            //save version
            await SaveHierarchyVersion(action);
            await _logService.Log(action, node.Name);


            //notification
            var currentUserId = GetCurrentUserID();
            var currentUser = GetCurrentUser();

            AssetNotificationDTO notification = new AssetNotificationDTO
            {
                User = currentUserId,
                Name = name,
                Type = "AssetDeleted"
            };
            await _notificationService.BroadcastToAdminsAndViewers(currentUserId, notification, notificationType);

            string notificationMessage = $"{GetCurrentUser()} deleted asset {name}";
            await _notificationStoreService.SaveNotificationsForOfflineUsers(notificationType, notificationMessage, int.Parse(currentUserId), currentUser);

            return true;
        }
        private async Task DeleteRecursively(Asset node)
        {
            await _assetRepository.LoadAssetWithChildrenAsync(node);

            foreach (var child in node.Children.ToList())
            {
                await DeleteRecursively(child);
            }

            // Mark node for deletion
            _assetRepository.RemoveAssetAsync(node);
        }


        public async Task<bool> UpdateNode(int oldId, string newName)
        {
            string action = "Update Asset";
            var node = await _assetRepository.GetAssetByIdAsync(oldId);
            if (node.ParentId == null) return false; //disallow updating root node.
            if (node == null) return false;
            //check if same exists elsewhere
            bool checkName = await _assetRepository.CheckAssetByNameAsync(newName); 

            if (!checkName)
            {
                string oldName = node.Name; //for notification purpose 

                node.Name = newName;
                await _assetRepository.SaveChangesAsync();


                //save version
                await SaveHierarchyVersion(action);
                await _logService.Log(action, newName);

                //notification
                var currentUserId = GetCurrentUserID();
                var currentUser = GetCurrentUser();
                AssetNotificationDTO notification = new AssetNotificationDTO
                {
                    User = currentUserId,
                    OldName = oldName,
                    NewName = newName,
                    Type = "AssetUpdated"
                };
                await _notificationService.BroadcastToAdminsAndViewers(currentUserId, notification, notificationType);

                string notificationMessage = $"{currentUser} updated asset {oldName} to {newName}";
                await _notificationStoreService.SaveNotificationsForOfflineUsers(notificationType, notificationMessage, int.Parse(currentUserId), currentUser);


                return true;
            }
            return false;



        }

        private Asset FindNodeByName(Asset node, string name)
        {

            if (node.Name.ToLower() == name.ToLower())
            {
                return node;

            }
            foreach (var child in node.Children)
            {

                var result = FindNodeByName(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;

        }
        private Asset? FindNodeById(Asset node, int id)
        {
            if (node.Id == id)
                return node;

            foreach (var child in node.Children)
            {
                var result = FindNodeById(child, id);
                if (result != null)
                    return result;
            }

            return null;
        }

        public int TotalAsset(Asset node)
        {
            return _assetRepository.GetAssetCount();
        }

        public bool CheckDuplicated(Asset node)
        {

            return true;
        }
        public async Task ReplaceTree(Asset newRoot)
        {

            // Check for duplicates in the incoming tree
            if (HasDuplicatesInTree(newRoot))
                throw new Exception("Duplicate nodes present in uploaded tree");

            // Check if incoming tree has a "Root" node
            if (ContainsRootNode(newRoot))
                throw new Exception("Node name \"Root\" present in the hierarchy.");

            try
            {
                // Try truncate table (faster, resets IDs)
                await _assetRepository.TruncateAssetsAsync();
            }
            catch
            {
                // Fallback: delete all one by one  + reseed identity
                await _assetRepository.DeleteAllAssetsAsync();
                await _assetRepository.ReseedAssetsIdentityAsync();
            }

            try
            {
                // Create new root
                string action = "Replace Hierarchy";
                var json = SerializeJson(newRoot); //json before saving to database for log purposes
                var root = new Asset { Name = "Root" };
                await _assetRepository.AddAsync(root);
                await _assetRepository.SaveChangesAsync();// Save to get the generated ID


                // Set parent relationships and add tree
                ResetIds(newRoot);
                SetParentIds(newRoot, root.Id);
                await _assetRepository.AddAsync(newRoot);
                await _assetRepository.SaveChangesAsync();

                //await _logService.Log(action, asset: json);
                await SaveHierarchyVersion(action);
            }
            catch (DbUpdateException ex)
            {

                throw;
            }



        }
        private void ResetIds(Asset asset)
        {
            asset.Id = 0;
            foreach (var s in asset.Signals)
                s.Id = 0;

            foreach (var child in asset.Children)
                ResetIds(child);
        }

        private bool ContainsRootNode(Asset root)
        {
            if (root.Name.ToLower() == "root")
                return true;
            foreach (var child in root.Children)
            {
                if (ContainsRootNode(child))
                    return true;
            }
            return false;

        }
        private void SetParentIds(Asset node, int id)
        {
            node.ParentId = id;
            node.Id = 0;

            foreach (var child in node.Children)
            {
                child.Id = 0;
                SetParentIds(child, 0); // Will be updated after parent is saved
            }
        }
        private bool HasDuplicatesInTree(Asset root)
        {
            var names = new HashSet<string>();
            return CheckDuplicatesRecursively(root, names);
        }

        private bool CheckDuplicatesRecursively(Asset node, HashSet<string> names)
        {
            if (!names.Add(node.Name.ToLower()))
                return true; // Duplicate found

            foreach (var child in node.Children)
            {
                if (CheckDuplicatesRecursively(child, names))
                    return true;
            }
            return false;
        }


        public async Task<int> TreeLength(Asset node)
        {
            int count = 1;

            await _assetRepository.LoadAssetWithChildrenAsync(node);
            foreach (var child in node.Children)
            {
                count += await TreeLength(child);
            }

            return count;
        }

        public async Task<int> MergeTree(Asset newTree)
        {
            int totalAdded = 0;
            string action = "Merge Hierarchy";
            var json = SerializeJson(newTree);
            // Global duplicate check in the incoming tree itself (before merge)
            bool hasDuplicates = HasDuplicatesInTree(newTree);
            if (hasDuplicates)
                throw new Exception("Duplicate nodes present in uploaded tree");

            // Get the actual DB root
            var dbRoot = await _assetRepository.GetRootWithChildrenAsync();
            if (dbRoot == null)
            {
                dbRoot = new Asset { Name = "Root" };
                await _assetRepository.AddAsync(dbRoot);
                await _assetRepository.SaveChangesAsync(); // Save to get generated ID
            }
            foreach (var child in newTree.Children)
            {
                totalAdded +=  await MergeNode(dbRoot, child);

            }


            if (totalAdded > 0)
            {
                await _assetRepository.SaveChangesAsync();
            }
                

            //recursivley load children in root to represent deep hierarchy
            var dbroot = await _assetRepository.GetRootWithChildrenAsync();

            //await _logService.Log(action, asset: json);

            await SaveHierarchyVersion(action);



            return totalAdded;
        }

        private async Task<int> MergeNode(Asset currentParent, Asset newNode)
        {
            // FIRST: Check if node exists GLOBALLY by name (most important check)
            var globalMatch =await _assetRepository.GetAssetByNameAsync(newNode.Name);

            if (globalMatch != null)
            {
                // Node exists somewhere - merge all children into it
                int addedCount = 0;

                foreach (var child in newNode.Children)
                {
                    addedCount += await MergeNode(globalMatch, child);
                }
                return addedCount;
            }

            // SECOND: If no global match, add as new node under current parent
            newNode.Id = 0; // Let EF generate new ID
            newNode.ParentId = currentParent.Id;

            // Reset all child IDs recursively
            ResetChildIds(newNode);

            await _assetRepository.AddAsync(newNode);

            assetsAdded.Add(newNode);

            return await TreeLength(newNode);
        }

        private void ResetChildIds(Asset node)
        {
            foreach (var child in node.Children)
            {
                child.Id = 0;
                child.ParentId = 0; // Will be set by EF Core navigation properties
                ResetChildIds(child);
            }
        }




    }
}