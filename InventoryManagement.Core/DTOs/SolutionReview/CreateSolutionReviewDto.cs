namespace InventoryManagement.Core.DTOs.SolutionReview;

public class CreateSolutionReviewDto
{
    public int TicketSolutionId { get; set; }
    public bool IsInTime { get; set; }
    public int? DelayReasonId { get; set; }
    public string Notes { get; set; }
    public TimeSpan? HowMuchLate { get; set; }
}