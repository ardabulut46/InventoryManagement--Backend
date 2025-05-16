namespace InventoryManagement.Core.Enums;

public enum ApprovalStatus
{
    Pending,    // Request is awaiting action
    Approved,   // Request has been approved
    Rejected,   // Request has been rejected
    Cancelled   // Request was cancelled by the requester before action was taken
} 