using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Enums;
using System.Threading.Tasks;

namespace InventoryManagement.Core.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(
        int recipientUserId,
        string message,
        NotificationType notificationType,
        string? relatedEntityType = null,
        int? relatedEntityId = null);

    // Future potential methods:
    // Task MarkAsReadAsync(int notificationId, int userId);
    // Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int userId);
    // Task SendNotificationAsync(Notification notification); // For actual delivery (email, SignalR etc.)
} 