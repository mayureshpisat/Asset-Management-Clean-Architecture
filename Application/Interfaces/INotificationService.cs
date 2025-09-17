using Application.DTO;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task BroadcastToAdminsAndViewers( string currentUserId,  AssetNotificationDTO notification, string? notificationType = null);
    }
}
