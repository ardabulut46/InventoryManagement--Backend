using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.Entities;

public class Inventory : BaseEntity
{
    public string Barcode { get; set; }
    public string SerialNumber { get; set; }
    public int? AssignedUserId { get; set; }
    public User AssignedUser { get; set; }

    public int? LastUserId { get; set; }
    public User LastUser { get; set; }
    
    public string? InvoiceAttachmentPath { get; set; }
    
    public string Family { get; set; }
    public string Type { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Location { get; set; }
    public int? PurchasePrice { get; set; }
    public PurchaseCurrency? PurchaseCurrency { get; set; }
    public string Status { get; set; }
    public string Room { get; set; }
    public string Floor { get; set; }
    public string Block { get; set; }
    public string Department { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    
    public DateTime? PurchaseDate { get; set; }
    
    public string Supplier { get; set; }
    public int? SupportCompanyId { get; set; }
    public Company SupportCompany { get; set; }

    public int? CreatedUserId { get; set; }
    public User? CreatedUser { get; set; }
   
    public ICollection<InventoryHistory> InventoryHistory { get; set; }
    
}