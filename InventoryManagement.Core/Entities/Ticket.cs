using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.Entities;

public class Ticket : BaseEntity
{
    public string RegistrationNumber { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; }
    //public int DepartmentId { get; set; }
    //public Department Department { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public string ProblemType { get; set; }
    public string? Location { get; set; }
    public string? Room { get; set; }
    public string Subject { get; set; }
    public int? InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? AttachmentPath { get; set; }

    public DateTime? AssignedDate { get; set; }
    public TimeSpan? IdleDuration { get; set; }
    
}