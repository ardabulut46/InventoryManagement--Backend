namespace InventoryManagement.Core.Entities;

public class SolutionTime : BaseEntity
{
    public int ProblemTypeId { get; set; } // it'll be dropdown on frontend
    public ProblemType ProblemType { get; set; }
    
    public TimeSpan TimeToSolve { get; set; }

}