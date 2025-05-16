using InventoryManagement.Core.Enums;
using System; // Required for DateTime

namespace InventoryManagement.Core.Entities;

public class Notification : BaseEntity // Assuming you have a BaseEntity with an Id property
{
    public int RecipientUserId { get; set; }
    public virtual User RecipientUser { get; set; }

    public string Message { get; set; }
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public NotificationType Type { get; set; }

    // Optional: To link notification to a specific entity that it relates to
    public string? RelatedEntityType { get; set; } // e.g., "Inventory", "ApprovalRequest"
    public int? RelatedEntityId { get; set; }    // ID of the related entity
} 