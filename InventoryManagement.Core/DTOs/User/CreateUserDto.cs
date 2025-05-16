using System.ComponentModel.DataAnnotations;
using InventoryManagement.Core.Entities;

namespace InventoryManagement.Core.DTOs.User;

public class CreateUserDto
{
    [Required(ErrorMessage = "İsim alanı zorunludur")]
    [StringLength(50)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Soyisim alanı zorunludur")]
    [StringLength(50)]
    public string Surname { get; set; }
    

    [Required(ErrorMessage = "Email alanı zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; }
    
    public string? Location { get; set; }
    
    public int? GroupId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
    
    public string? Floor { get; set; }
    public string? Room { get; set; }

    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
    public int? RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}