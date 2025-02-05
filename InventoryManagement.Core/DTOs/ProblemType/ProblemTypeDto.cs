namespace InventoryManagement.Core.DTOs.ProblemType;

public class ProblemTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public bool IsActive { get; set; }
}