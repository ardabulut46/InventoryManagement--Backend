namespace InventoryManagement.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedById { get; set; }
    
    public bool IsActive { get; set; } = true;
    
}