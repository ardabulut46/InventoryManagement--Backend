namespace InventoryManagement.Core.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = "";
    public int DepartmentId { get; set; }
    public virtual Department Department { get; set; }
    public virtual ICollection<User> Users { get; set; }

    // New properties for Group Manager
    public int? ManagerId { get; set; }
    public virtual User Manager { get; set; }
}