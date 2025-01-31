namespace InventoryManagement.Core.DTOs.SolutionTime;

public class CreateUpdateSolutionTimeDto
{
    public int ProblemTypeId { get; set; }
    public TimeSpan TimeToSolve { get; set; }
}
