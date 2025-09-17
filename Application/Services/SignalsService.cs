using Application.Interfaces;
using Application.DTO;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Security.Claims;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

namespace Application.Services
{
    public enum SignalAction
    {
        AddSignal = 1,
        UpdateSignal = 2,
        DeleteSignal = 3,
    }
    public class SignalsService : ISignalsService
    {
        private readonly IAssetStorageService _storage;
        private readonly IAssetLogService _logger;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISignalRepository _signalRepository;
        
        public SignalsService(IAssetStorageService storage, IAssetLogService logger, IHttpContextAccessor httpContextAccessor, ISignalRepository signalRepository, INotificationService notificationService)
        {
            _storage = storage;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _signalRepository = signalRepository;
            _notificationService = notificationService;
        }
        

        private string? GetCurrentUserID()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        }
        private string? GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
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
        public async Task<IEnumerable<Signal>> GetSignals(int assetId)
        {
            var asset = await _signalRepository.GetAssetWithSignalsAsync(assetId);
            if (asset == null)
                throw new Exception("Asset not found");
            List<Signal> signals = asset.Signals.ToList();
            foreach(var signal in signals)
            {
                Console.WriteLine($"{signal.Name}");
            }
            

            return signals;
        }
        public async Task<Signal> GetSpecificSignal(int assetId, int signalId)
        {
            var asset = await _signalRepository.GetAssetWithSignalsAsync(assetId);
            if (asset == null)
                throw new Exception("Asset not found");
            var signal = asset.Signals.FirstOrDefault(s=>s.Id == signalId);
            if (signal == null)
                throw new Exception("Signal not found");
            return signal;
        }

        

        private async Task SaveHierarchyVersion(string? action = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                action = "None";
            //in memory objects reflect db state
            //saving in file for downloading and tracking purpose.
            //recursivley load children in root to represent deep hierarchy
            var root = await _signalRepository.GetRootWithChildrenAsync();
            _storage.SaveTree(root, action);
        }
        

        
        public async Task AddSignal(int assetId, GlobalSignalDTO signal)
        {
            var action = "Add Signal";
            var asset = await _signalRepository.GetAssetAsync(assetId);
            if (asset == null)
                throw new Exception("Asset not found");
            string parentName = asset.Name;
            asset.Signals.Add(new Signal { Name = signal.Name, ValueType = signal.ValueType, Description = signal.Description });
            await _signalRepository.SaveChangesAsync();
            await SaveHierarchyVersion(action: "Add Signal");
            await _logger.Log(action, null, signal.Name);


            //notification
            var currentUserId = GetCurrentUserID();
            var currentUser = GetCurrentUser();
            AssetNotificationDTO signalNotification = new AssetNotificationDTO
            {
                Type = action,
                User = currentUser,
                Name = signal.Name,
                ParentName = parentName
            };
            await _notificationService.BroadcastToAdminsAndViewers(currentUserId, signalNotification);


        }
        public async Task UpdateSignal(int assetId, int signalId, GlobalSignalDTO request)
        {
            string action = "Update Signal";
            //write Include as EF core uses lazy loading by default, i.e navigataional properties of Signals are not loaded
            var asset = await _signalRepository.GetAssetWithSignalsAsync(assetId);
            if (asset == null)
                throw new Exception("Asset not found");
            var signal = asset.Signals.FirstOrDefault(s => s.Id == signalId);
            if (signal == null)
                throw new Exception("Signal not found");
            
            var oldName = signal.Name; //notif
            var newName = request.Name;
            string parentName = asset.Name;

            signal.Name = request.Name;
            signal.Description = request.Description;
            signal.ValueType = request.ValueType;
            await _signalRepository.SaveChangesAsync();
            await SaveHierarchyVersion( action: "Update Signal");
            await _logger.Log(action, null, signal: request.Name);


            //notification
            var currentUserId = GetCurrentUserID();
            var currentUser = GetCurrentUser();
            AssetNotificationDTO signalNotification = new AssetNotificationDTO
            {
                Type = action,
                User = currentUser,
                OldName = oldName,
                NewName = newName,
                ParentName = parentName
            };
            await _notificationService.BroadcastToAdminsAndViewers(currentUserId, signalNotification);

        }

        public async Task DeleteSignal(int signalId, int assetId)
        {
            var action = "Delete Signal";
            var asset = await _signalRepository.GetAssetWithSignalsAsync(assetId);
            if (asset == null)
                throw new Exception("Asset not found");
            var signal = asset.Signals.FirstOrDefault(s => s.Id == signalId);
            if (signal == null)
                throw new Exception("Signal not found");


            string signalName = signal.Name; //for sending notification
            string parentName = asset.Name;
            Console.WriteLine($"FROM DELETE SIGNAL " + signalName);
            //do deletion
            await _signalRepository.RemoveSignalAsync(signal);
            await SaveHierarchyVersion(action: "Delete Signal");
            await _logger.Log(action, null, signal: signal.Name);

            var currentUserId = GetCurrentUserID();
            var currentUser = GetCurrentUser();
            AssetNotificationDTO signalNotification = new AssetNotificationDTO
            {
                Type = action,
                User = currentUser,
                Name = signalName,
                ParentName = parentName
            };
            await _notificationService.BroadcastToAdminsAndViewers(currentUserId, signalNotification);

            string notificationMessage = $"{GetCurrentUser()} deleted signal {signal.Name} under {parentName}";
        }




    }

}
