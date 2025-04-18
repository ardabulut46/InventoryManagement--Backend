using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Core.Entities;

public class SolutionTime : BaseEntity
{
    public int ProblemTypeId { get; set; } 
    public virtual ProblemType ProblemType { get; set; }
    
    public TimeSpan TimeToSolve { get; set; }

}