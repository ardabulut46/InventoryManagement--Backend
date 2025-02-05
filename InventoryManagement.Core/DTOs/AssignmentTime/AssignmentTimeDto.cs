namespace InventoryManagement.Core.DTOs.AssignmentTime;

public class AssignmentTimeDto
{
    public int Id { get; set; }
    public int ProblemTypeId { get; set; }
    public string ProblemTypeName { get; set; }
    public TimeSpan TimeToAssign { get; set; }
}