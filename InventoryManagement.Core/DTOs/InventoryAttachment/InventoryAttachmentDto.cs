namespace InventoryManagement.Core.DTOs.InventoryAttachment;

public class InventoryAttachmentDto
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public string Description { get; set; }
}