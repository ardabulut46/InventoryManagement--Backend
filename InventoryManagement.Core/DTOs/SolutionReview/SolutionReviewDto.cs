namespace InventoryManagement.Core.DTOs.SolutionReview;

public class SolutionReviewDto
{
    public int Id { get; set; }
    public int TicketSolutionId { get; set; }
    public bool IsInTime { get; set; }
    public int? DelayReasonId { get; set; }
    public string Notes { get; set; }
    public TimeSpan HowLate { get; set; }
}