namespace InventoryManagement.Core.Entities;

public class TicketNote : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; }

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }

    public string Note { get; set; }
    public string? AttachmentPath { get; set; }
    
    // Optional: Add note type/category for future use
    public string? NoteType { get; set; }
    
    public ICollection<TicketNoteAttachment> Attachments { get; set; } = new List<TicketNoteAttachment>();
}