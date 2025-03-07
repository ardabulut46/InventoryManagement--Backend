namespace InventoryManagement.Core.Entities;

public class InventoryAttachment : BaseEntity
{
    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;
    public string? Description { get; set; }
}