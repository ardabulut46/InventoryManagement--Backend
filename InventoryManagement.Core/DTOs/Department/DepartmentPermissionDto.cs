namespace InventoryManagement.Core.DTOs.Department;

public class DepartmentPermissionDto
{
    public int DepartmentId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}