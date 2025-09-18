using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Hubs;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class NotificationStoreService : INotificationStoreService
    {
        private readonly AssetDbContext _dbContext;
        
        public NotificationStoreService(AssetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //from dbassethierarchyservice
        public async Task SaveNotificationsForOfflineUsers(string type, string notificationMessage, int senderId, string senderName)
        {
            // Get all admins except the sender
            var allAdmins = await _dbContext.Users.Where(u => u.Role == "Admin" && u.Id != senderId).ToListAsync();

            foreach (var admin in allAdmins)
            {
                // Check if admin is currently online
                List<string> adminConnections = NotificationHub.GetConnections(admin.Id.ToString());
                bool isOnline = adminConnections != null && adminConnections.Any();

                var notification = new Notification
                {
                    UserId = admin.Id,
                    Type = type,
                    Message = notificationMessage,
                    SenderName = senderName,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                Console.WriteLine($"Saving notification for Admin ID: {admin.Id} (Online: {isOnline})");
                Console.WriteLine($"Sender: {notification.SenderName}");
                Console.WriteLine($"Message: {notification.Message}");
                _dbContext.Notifications.Add(notification);
            }

            if (allAdmins.Any())
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
