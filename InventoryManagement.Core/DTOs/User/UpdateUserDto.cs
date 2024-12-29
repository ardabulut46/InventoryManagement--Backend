using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Core.DTOs.User;

public class UpdateUserDto
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

    [Required(ErrorMessage = "Lokasyon alanı zorunludur")]
    public string Location { get; set; }

    [Required(ErrorMessage = "Departman alanı zorunludur")]
    public string Department { get; set; }

    public bool IsActive { get; set; }
}