namespace InventoryManagement.Core.DTOs.Ticket;

public class UserTicketCountDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } // concat name surname
    public int TicketCount { get; set; }
}