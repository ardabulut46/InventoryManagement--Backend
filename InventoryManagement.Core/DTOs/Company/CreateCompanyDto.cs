using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Core.DTOs.Company;

public class CreateCompanyDto
{
    [Required(ErrorMessage = "Firma adı zorunludur")]
    [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Adres zorunludur")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Email zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Telefon numarası zorunludur")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string Phone { get; set; }
}