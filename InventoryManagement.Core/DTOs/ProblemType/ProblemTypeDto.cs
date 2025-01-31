namespace InventoryManagement.Core.DTOs.ProblemType;

public class ProblemTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public bool IsActive { get; set; }
}