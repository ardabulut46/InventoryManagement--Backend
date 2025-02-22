namespace InventoryManagement.Core.DTOs.TicketHistory;

public class TicketHistoryDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string TicketRegistrationNumber { get; set; }
    public string UserEmail { get; set; }
    public int? FromAssignedUserId { get; set; }
    public string FromAssignedUserEmail { get; set; }
    public int? ToUserId { get; set; }
    public string ToUserEmail { get; set; }
    public int? GroupId { get; set; }
    public string GroupName { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
}