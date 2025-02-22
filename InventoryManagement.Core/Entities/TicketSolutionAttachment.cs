namespace InventoryManagement.Core.Entities;

public class TicketSolutionAttachment : BaseEntity
{
    public int TicketSolutionId { get; set; }
    public TicketSolution TicketSolution { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;
    
}