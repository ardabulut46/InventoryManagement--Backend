using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Core.DTOs.Ticket;

public class CreateTicketDto
{
    [Required(ErrorMessage = "Kayıt numarası zorunludur")]
    public string RegistrationNumber { get; set; }

    [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
    public int UserId { get; set; }
    
    public int? InventoryId { get; set; }

    [Required(ErrorMessage = "Problem tipi zorunludur")]
    public string ProblemType { get; set; }

    public string Location { get; set; }
    public string Room { get; set; }

    [Required(ErrorMessage = "Konu bilgisi zorunludur")]
    public string Subject { get; set; }
    
    [Required(ErrorMessage = "Açıklama zorunludur")]
    public string Description { get; set; }

    public string Status { get; set; }
    public string AttachmentPath { get; set; }
}