namespace InventoryManagement.Core.DTOs.TicketNote;

public class TicketNoteAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
}