namespace InventoryManagement.Core.Entities;

public class SolutionReview : BaseEntity
{
    public int TicketSolutionId { get; set; }
    public TicketSolution TicketSolution { get; set; }

    public bool IsInTime { get; set; }
    
    public int? DelayReasonId { get; set; }
    public DelayReason? DelayReason { get; set; }
    
    public string Notes { get; set; }

    public TimeSpan? HowMuchLate { get; set; }
    
}