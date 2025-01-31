namespace InventoryManagement.Core.DTOs.SolutionTime;

public class SolutionTimeDto
{
    public int Id { get; set; }
    public int ProblemTypeId { get; set; }
    public string ProblemTypeName { get; set; }
    public TimeSpan TimeToSolve { get; set; }
}