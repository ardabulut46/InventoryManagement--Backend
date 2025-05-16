namespace InventoryManagement.Core.DTOs.Group;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public int? ManagerId { get; set; }
}