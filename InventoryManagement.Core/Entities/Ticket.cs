namespace InventoryManagement.Core.Entities;

public class Ticket : BaseEntity
{
    public string RegistrationNumber { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string ProblemType { get; set; }
    public string Location { get; set; }
    public string Room { get; set; }
    public string Subject { get; set; }
    public int? InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string AttachmentPath { get; set; }
    
}