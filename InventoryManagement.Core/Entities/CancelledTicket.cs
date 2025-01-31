namespace InventoryManagement.Core.Entities;

public class CancelledTicket : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int CancelReasonId { get; set; }
    public CancelReason CancelReason { get; set; }
}