namespace InventoryManagement.Core.DTOs.UsersAssignedTickets;

public class UsersAssignedTicketsCreateUpdateDto
{
    public int UserId { get; set; }
    public int SolvedTicketCount { get; set; }
    public int CancelledTicketCount { get; set; }
    public int LateSolvingCount { get; set; }
}