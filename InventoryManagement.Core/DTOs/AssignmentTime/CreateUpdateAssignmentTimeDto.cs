namespace InventoryManagement.Core.DTOs.AssignmentTime;

public class CreateUpdateAssignmentTimeDto
{
    public int ProblemTypeId { get; set; }
    public TimeSpan TimeToAssign { get; set; }
}