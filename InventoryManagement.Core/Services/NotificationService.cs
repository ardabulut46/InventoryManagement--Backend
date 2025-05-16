using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace InventoryManagement.Core.Services;

public class NotificationService : INotificationService
{
    private readonly IGenericRepository<Notification> _notificationRepository;
    // Potentially inject IEmailService or similar for actual sending in the future

    public NotificationService(IGenericRepository<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task CreateNotificationAsync(
        int recipientUserId,
        string message,
        NotificationType notificationType,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message), "Notification message cannot be empty.");

        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Message = message,
            Type = notificationType,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            SentDate = DateTime.UtcNow,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
        // In a more advanced scenario, you might also trigger an event here,
        // or directly call another service to send the notification (email, SignalR push, etc.)
        // For example: await _emailService.SendNotificationEmailAsync(notification);
    }
} 