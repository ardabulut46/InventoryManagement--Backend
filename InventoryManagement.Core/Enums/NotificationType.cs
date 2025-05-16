namespace InventoryManagement.Core.Enums;

public enum NotificationType
{
    Information,        // General information
    ApprovalRequest,    // A new request needs attention
    ApprovalApproved,   // A request was approved
    ApprovalRejected,   // A request was rejected
    ActionRequired,     // Some other action is required
    Reminder            // A reminder notification
} 