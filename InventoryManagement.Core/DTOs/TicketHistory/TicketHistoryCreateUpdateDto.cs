namespace InventoryManagement.Core.DTOs.TicketHistory;

public class TicketHistoryCreateUpdateDto
{
    public int TicketId { get; set; }
    public int FromAssignedUserId { get; set; }
    public int? ToUserId { get; set; }
    public int? GroupId { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
}