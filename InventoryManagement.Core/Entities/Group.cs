namespace InventoryManagement.Core.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = "";
    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    public ICollection<User> Users { get; set; }
}