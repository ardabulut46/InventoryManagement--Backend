using Microsoft.AspNetCore.Identity;

namespace InventoryManagement.Core.Entities;

public class User : IdentityUser<int>
{
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string? Location { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
}