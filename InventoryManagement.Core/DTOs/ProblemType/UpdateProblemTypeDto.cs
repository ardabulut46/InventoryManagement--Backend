namespace InventoryManagement.Core.DTOs.ProblemType;

public class UpdateProblemTypeDto
{
    public string Name { get; set; }
    public int GroupId { get; set; }
    public bool IsActive { get; set; }
}