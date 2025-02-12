namespace InventoryManagement.Core.Entities;

public class IdleDurationLimit : BaseEntity
{
    public int ProblemTypeId { get; set; } 
    public virtual ProblemType ProblemType { get; set; }
    
    public TimeSpan TimeToAssign { get; set; }
    
}