using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Hubs;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(IHubContext<NotificationHub> hubContext, IHttpContextAccessor httpContextAccessor) 
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
        }

        
        public async Task BroadcastToAdminsAndViewers( string currentUserId,  AssetNotificationDTO notification, string? notificationType = null)
        {
            var connectionIds = NotificationHub.GetConnections(currentUserId) ?? new List<string>();

            Console.WriteLine("FROM BROADCAST");
            Console.WriteLine(currentUserId);
            Console.WriteLine($"{notification.Name}");
            foreach(string id in connectionIds)
            {
                Console.WriteLine(id);
            }

            // viewers (mask user as "Admin")
            var viewerNotification = new AssetNotificationDTO
            {
                Type = notification.Type,
                User = "Admin", // mask
                Name = notification.Name,
                OldName = notification.OldName,
                NewName = notification.NewName,
                ParentName = notification.ParentName
            };

            if (notificationType.ToLower() == "signal")
            {
                // admins (exclude actor)
                await _hubContext.Clients
                    .GroupExcept("Role_Admin", connectionIds)
                    .SendAsync("RecieveSignalNotification", notification);

                //viewer
                await _hubContext.Clients
                    .Group("Role_Viewer")
                    .SendAsync("RecieveSignalNotification", viewerNotification);

            }
            else
            {
                // admins (exclude actor)
                await _hubContext.Clients
                    .GroupExcept("Role_Admin", connectionIds)
                    .SendAsync("RecieveAssetNotification", notification);

                //viewer
                await _hubContext.Clients
                    .Group("Role_Viewer")
                    .SendAsync("RecieveAssetNotification", viewerNotification);

            }


            
        }

        public async Task SendStatsToEveryone(double tempAvg, double powerAvg)
        {
            var currentUserConIds =  _hubContext.Clients.All.SendAsync("RecieveStatsNotification", tempAvg, powerAvg);
        }







        //private async Task SaveNotificationsForOfflineUsers(string type, string notificationMessage, int senderId, string senderName)
        //{
        //    // Get all admins except the sender
        //    var allAdmins = await _dbContext.Users.Where(u => u.Role == "Admin" && u.Id != senderId).ToListAsync();

        //    foreach (var admin in allAdmins)
        //    {
        //        // Check if admin is currently online
        //        List<string> adminConnections = NotificationHub.GetConnections(admin.Id.ToString());
        //        bool isOnline = adminConnections != null && adminConnections.Any();

        //        var notification = new Notification
        //        {
        //            UserId = admin.Id,
        //            Type = type,
        //            Message = notificationMessage,
        //            SenderName = senderName,
        //            CreatedAt = DateTime.UtcNow,
        //            IsRead = isOnline // Mark as read if user is currently online
        //        };

        //        Console.WriteLine($"Saving notification for Admin ID: {admin.Id} (Online: {isOnline})");
        //        Console.WriteLine($"Sender: {notification.SenderName}");
        //        Console.WriteLine($"Message: {notification.Message}");
        //        _dbContext.Notifications.Add(notification);
        //    }

        //    if (allAdmins.Any())
        //    {
        //        await _dbContext.SaveChangesAsync();
        //    }
        //}
    }
}
