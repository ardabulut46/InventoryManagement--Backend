namespace InventoryManagement.Core.DTOs.ProblemType;

public class UpdateProblemTypeDto
{
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
}