namespace InventoryManagement.Core.DTOs.TicketSolution;

public class TicketSolutionCreateUpdateDto
{
    public int TicketId { get; set; }

    //public int AssignedUserId { get; set; }
    public string Subject { get; set; }
    public string Description { get; set; }
    public int SolutionTypeId { get; set; }
    public bool IsChronicle { get; set; }
    public string? AttachmentPath { get; set; }
}
