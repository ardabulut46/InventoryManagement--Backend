namespace InventoryManagement.Core.DTOs.Role;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}