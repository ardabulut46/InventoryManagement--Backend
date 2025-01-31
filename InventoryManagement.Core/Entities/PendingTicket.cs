using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace InventoryManagement.Core.Entities;

public class PendingTicket : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int PendingReasonId { get; set; }
    public PendingReason PendingReason { get; set; }
}