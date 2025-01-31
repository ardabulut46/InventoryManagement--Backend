namespace InventoryManagement.Core.Entities;

public class UsersAssignedTickets : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int SolvedTicketCount { get; set; }
    public int CancelledTicketCount { get; set; }
    public int LateSolvingCount { get; set; }
    
}