using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface INotificationStoreService
    {
        Task SaveNotificationsForOfflineUsers(string notificationType, string notificationMessage, int senderId, string senderName);

    }
}
