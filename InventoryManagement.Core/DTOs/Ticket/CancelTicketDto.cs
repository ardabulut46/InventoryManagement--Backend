namespace InventoryManagement.Core.DTOs.Ticket;

public class CancelTicketDto
{
    public int CancelReasonId { get; set; }
    public string? Notes { get; set; }
}