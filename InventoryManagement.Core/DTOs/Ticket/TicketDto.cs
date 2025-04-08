using InventoryManagement.Core.DTOs.Group;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.DTOs.Ticket;

public class TicketDto
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public GroupDto Group { get; set; }
    public UserDto User { get; set; }  
    public int? InventoryId { get; set; }
    public InventoryDto Inventory { get; set; }
    public int ProblemTypeId { get; set; }
    public string ProblemTypeName { get; set; }
    public string Location { get; set; }
    public string Room { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string PriorityLabel => Priority.ToString();
    public string AttachmentPath { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int CreatedById { get; set; }
    public UserDto CreatedBy { get; set; }
    public DateTime AssignedDate { get; set; }
    public TimeSpan? IdleDuration { get; set; }
    public string IdleDurationDisplay { get; set; }
    public TimeSpan? TimeToAssign { get; set; }
    public string TimeToAssignDisplay { get; set; }
    public bool IsAssignmentOverdue => !UserId.Equals(default) 
        ? false 
        : TimeToAssign.HasValue && (DateTime.Now - CreatedDate) > TimeToAssign.Value;
    
}