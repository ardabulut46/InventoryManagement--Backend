namespace InventoryManagement.Core.DTOs.TransferTicket;

public class TransferTicketDto
{
    public int TicketId { get; set; }
    public int GroupId { get; set; }
    public string? Subject { get; set; }
    public string? Description { get; set; }
}