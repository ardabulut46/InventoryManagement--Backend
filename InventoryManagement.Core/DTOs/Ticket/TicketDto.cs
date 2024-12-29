using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.DTOs.User;

namespace InventoryManagement.Core.DTOs.Ticket;

public class TicketDto
{
    public int Id { get; set; }
    public string RegistrationNumber { get; set; }
    public int UserId { get; set; }
    public UserDto User { get; set; }  
    public int? InventoryId { get; set; }
    public InventoryDto Inventory { get; set; }
    public string ProblemType { get; set; }
    public string Location { get; set; }
    public string Room { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string AttachmentPath { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}