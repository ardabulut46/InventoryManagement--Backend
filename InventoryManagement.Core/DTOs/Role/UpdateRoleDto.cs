namespace InventoryManagement.Core.DTOs.Role;

public class UpdateRoleDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}