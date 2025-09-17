using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(IHubContext<NotificationHub> hubContext) 
        {
            _hubContext = hubContext;
        }
        public async Task BroadcastToAdminsAndViewers(string currentUserId,  AssetNotificationDTO notification)
        {
            var connectionIds = NotificationHub.GetConnections(currentUserId) ?? new List<string>();

            Console.WriteLine("FROM BROADCAST");
            Console.WriteLine(currentUserId);
            Console.WriteLine($"{notification.Name}");
            foreach(string id in connectionIds)
            {
                Console.WriteLine(id);
            }
            // admins (exclude actor)
            await _hubContext.Clients
                .GroupExcept("Role_Admin", connectionIds)
                .SendAsync("RecieveAssetNotification", notification);

            // viewers (mask user as "Admin")
            var viewerNotification = new AssetNotificationDTO
            {
                Type = notification.Type,
                User = "Admin", // mask
                Name = notification.Name,
                OldName = notification.OldName,
                NewName = notification.NewName
            };

            await _hubContext.Clients
                .Group("Role_Viewer")
                .SendAsync("RecieveAssetNotification", viewerNotification);
        }



        //from dbassethierarchyservice
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
        //            IsRead = false
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
