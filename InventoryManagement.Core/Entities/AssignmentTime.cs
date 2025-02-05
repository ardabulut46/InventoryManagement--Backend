namespace InventoryManagement.Core.Entities;

public class AssignmentTime : BaseEntity
{
    public int ProblemTypeId { get; set; }
    public ProblemType ProblemType { get; set; }
    
    public TimeSpan TimeToAssign{ get; set; }
}