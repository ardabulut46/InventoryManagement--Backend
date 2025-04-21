using System.ComponentModel.DataAnnotations;
using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.DTOs.Ticket;

public class UpdateTicketDto
{
    [Required(ErrorMessage = "Problem tipi zorunludur")]
    public int ProblemTypeId { get; set; }

    public string Location { get; set; }
    public string Room { get; set; }

    [Required(ErrorMessage = "Konu bilgisi zorunludur")]
    public string Subject { get; set; }

    public int? InventoryId { get; set; }

    [Required(ErrorMessage = "Açıklama zorunludur")]
    public string Description { get; set; }

    public string Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string AttachmentPath { get; set; }
}