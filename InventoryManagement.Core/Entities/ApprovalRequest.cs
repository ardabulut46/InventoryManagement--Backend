using InventoryManagement.Core.Enums;
using System;

namespace InventoryManagement.Core.Entities;

public class ApprovalRequest : BaseEntity // Assuming BaseEntity provides an Id
{
    public int RequestingUserId { get; set; }
    public virtual User RequestingUser { get; set; }

    // This will be the Group's ManagerId
    public int ApproverUserId { get; set; } 
    public virtual User ApproverUser { get; set; }

    public string EntityType { get; set; } // e.g., "Inventory", "Document"
    public int EntityId { get; set; }       // ID of the entity requiring approval
    public string ActionType { get; set; }   // e.g., "Delete", "Publish", "TransferOwner"

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ActionDate { get; set; } // When it was approved/rejected
    
    public string? RequesterComments { get; set; } // Comments from the user requesting approval
    public string? ApproverComments { get; set; } // Comments from the manager when approving/rejecting
} 