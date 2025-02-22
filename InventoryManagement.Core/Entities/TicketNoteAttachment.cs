namespace InventoryManagement.Core.Entities;

public class TicketNoteAttachment
{
    public int Id { get; set; }
    public int TicketNoteId { get; set; }
    public TicketNote TicketNote { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;
}