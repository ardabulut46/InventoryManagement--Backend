namespace InventoryManagement.Core.DTOs.TicketSolution;

public class TicketSolutionDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int UserId { get; set; }
   // public int AssignedUserId { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public int SolutionTypeId { get; set; }
    public DateTime SolutionDate { get; set; }
}