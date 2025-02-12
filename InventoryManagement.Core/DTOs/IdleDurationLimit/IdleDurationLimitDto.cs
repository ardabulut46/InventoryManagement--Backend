namespace InventoryManagement.Core.DTOs.IdleDurationLimit;

public class IdleDurationLimitDto
{
    public int Id { get; set; }
    public int ProblemTypeId { get; set; }
    public string ProblemTypeName { get; set; }
    public TimeSpan TimeToAssign { get; set; }
}