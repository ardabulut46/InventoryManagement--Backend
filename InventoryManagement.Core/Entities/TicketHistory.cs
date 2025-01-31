namespace InventoryManagement.Core.Entities;

public class TicketHistory : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int FromAssignedUserId { get; set; }
    public User FromAssignedUser { get; set; }

    public int? ToUserId { get; set; }
    public User? ToUser { get; set; }

    public int? GroupId { get; set; }
    public Group? Group { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    
    

}