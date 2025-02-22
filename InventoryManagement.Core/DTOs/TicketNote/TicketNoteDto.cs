namespace InventoryManagement.Core.DTOs.TicketNote;

public class TicketNoteDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string TicketRegistrationNumber { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByEmail { get; set; }
    public string Note { get; set; }
    public string? NoteType { get; set; }
    public DateTime CreatedDate { get; set; }
    public ICollection<TicketNoteAttachmentDto> Attachments { get; set; } = new List<TicketNoteAttachmentDto>();
}