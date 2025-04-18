using InventoryManagement.Core.DTOs.Department;
using InventoryManagement.Core.DTOs.Permission;

namespace InventoryManagement.Core.DTOs.User;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Location { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }
    
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public PermissionDto Permissions { get; set; }
    public string Role { get; set; }
    public IEnumerable<string> RolePermissions { get; set; } = new List<string>();
    public int RoleId { get; set; }
    public DepartmentDto Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}