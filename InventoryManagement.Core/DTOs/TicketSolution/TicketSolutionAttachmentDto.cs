namespace InventoryManagement.Core.DTOs.TicketSolution;

public class TicketSolutionAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
}