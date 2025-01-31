namespace InventoryManagement.Core.Entities;

public class DepartmentPermission
{
    public int Id { get; set; }

    // Hangi departmana ait?
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}