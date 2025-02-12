namespace InventoryManagement.Core.DTOs.IdleDurationLimit;

public class CreateUpdateIdleDurationLimitDto
{
    public int ProblemTypeId { get; set; }
    public TimeSpan TimeToAssign { get; set; }
}