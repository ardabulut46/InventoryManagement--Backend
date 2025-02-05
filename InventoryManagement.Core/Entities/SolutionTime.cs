namespace InventoryManagement.Core.Entities;

public class SolutionTime : BaseEntity
{
    public int ProblemTypeId { get; set; } 
    public ProblemType ProblemType { get; set; }
    
    public TimeSpan TimeToSolve { get; set; }

}