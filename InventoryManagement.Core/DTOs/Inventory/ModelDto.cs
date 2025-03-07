namespace InventoryManagement.Core.DTOs.Inventory;

public class ModelDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; }
    public bool IsActive { get; set; }
}