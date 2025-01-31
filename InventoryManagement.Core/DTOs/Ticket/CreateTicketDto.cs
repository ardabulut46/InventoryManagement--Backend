using System.ComponentModel.DataAnnotations;
using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.DTOs.Ticket;

public class CreateTicketDto
{
    
    public string? RegistrationNumber { get; set; }

    [Required(ErrorMessage = "Problem tipi zorunludur")]
    public int ProblemTypeId { get; set; }
    
    public int? InventoryId { get; set; }
    
    [Required(ErrorMessage = "Konu bilgisi zorunludur")]
    public string Subject { get; set; }
    
    [Required(ErrorMessage = "Açıklama zorunludur")]
    public string Description { get; set; }

    public string Status { get; set; }
    
    [Required(ErrorMessage = "Öncelik seviyesi zorunludur")]
    public TicketPriority Priority { get; set; }
    public string AttachmentPath { get; set; }
    public TimeSpan? IdleDuration { get; set; }
}