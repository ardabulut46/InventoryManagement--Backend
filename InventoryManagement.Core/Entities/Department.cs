namespace InventoryManagement.Core.Entities;

public class Department : BaseEntity
{
    
    public string Name { get; set; } = "";
    public ICollection<Group> Groups { get; set; }
}