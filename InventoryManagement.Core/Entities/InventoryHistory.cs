namespace InventoryManagement.Core.Entities;

public class InventoryHistory : BaseEntity
{
    public int InventoryId { get; set; }
    public int UserId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Notes { get; set; }
    public Inventory Inventory { get; set; }
    public User User { get; set; }
}