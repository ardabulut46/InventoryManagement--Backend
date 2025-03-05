using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Core.Entities;

public class Role : IdentityRole<int>
{
    public string Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}