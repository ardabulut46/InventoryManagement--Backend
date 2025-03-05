namespace InventoryManagement.Core.DTOs.Role;

public class CreateRoleDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}