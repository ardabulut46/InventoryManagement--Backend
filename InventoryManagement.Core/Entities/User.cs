using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Core.Entities;

public class User : IdentityUser<int>
{
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string? Location { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? Floor { get; set; }
    public string? Room { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public int? GroupId { get; set; }
    public Group? Group { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
}