using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Core.DTOs.Group;

public class CreateGroupDto
{
    [Required(ErrorMessage = "Grup adı zorunludur")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Departman zorunludur")]
    public int DepartmentId { get; set; }
}