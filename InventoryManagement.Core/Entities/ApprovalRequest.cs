using InventoryManagement.Core.Enums;
using System;

namespace InventoryManagement.Core.Entities;

public class ApprovalRequest : BaseEntity 
{
    public int RequestingUserId { get; set; }
    public virtual User RequestingUser { get; set; }

    // This will be the Group's ManagerId
    public int ApproverUserId { get; set; } 
    public virtual User ApproverUser { get; set; }

    public string EntityType { get; set; } 
    public int EntityId { get; set; }       
    public string ActionType { get; set; }   

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ActionDate { get; set; } 
    
    public string? RequesterComments { get; set; } 
    public string? ApproverComments { get; set; } 
} 