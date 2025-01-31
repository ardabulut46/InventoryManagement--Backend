namespace InventoryManagement.Core.Entities;

public class ProblemType : BaseEntity
{
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } // group'a güncellenecek
    public bool IsActive { get; set; } = true;
}