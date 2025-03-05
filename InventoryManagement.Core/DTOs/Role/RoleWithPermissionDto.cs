using InventoryManagement.Core.DTOs.Permission;

namespace InventoryManagement.Core.DTOs.Role;

public class RoleWithPermissionsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<RolePermissionDto> Permissions { get; set; } = new List<RolePermissionDto>();
}