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
    public ICollection<InventoryAttachment> Attachments { get; set; }
    
    public int? FamilyId { get; set; }
    public Family? Family { get; set; }
    
    public int? TypeId { get; set; }
    public InventoryType? Type { get; set; }
    
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    
    public int? ModelId { get; set; }
    public Model? Model { get; set; }
    public int? PurchasePrice { get; set; }
    public PurchaseCurrency? PurchaseCurrency { get; set; }
    public string Status { get; set; }
    public string Room { get; set; }
    public string Floor { get; set; }
    public string Department { get; set; }
    public string Location { get; set; }
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