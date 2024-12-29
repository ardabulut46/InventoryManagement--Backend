using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Core.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(50)]
    public string Surname { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(30)]
    public string Username { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; }

    public string? Location { get; set; }
    public string? Department { get; set; }
}

