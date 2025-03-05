namespace InventoryManagement.Core.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public string Permission { get; set; } // Format: "Resource:Action"
}