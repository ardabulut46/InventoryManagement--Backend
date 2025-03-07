using InventoryManagement.Core.DTOs.Company;
using InventoryManagement.Core.DTOs.InventoryAttachment;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.DTOs.Inventory;

public class InventoryDto
{
    public int Id { get; set; }
    public string Barcode { get; set; }
    public string SerialNumber { get; set; }
    public int? AssignedUserId { get; set; }
    public int? CreatedUserId { get; set; }
    public UserDto CreatedUser { get; set; }
    public UserDto AssignedUser { get; set; }
    public UserDto LastUser { get; set; }

    public string InvoiceAttachmentPath { get; set; }
    
    public int? SupportCompanyId { get; set; }
    public int PurchasePrice { get; set; }
    public PurchaseCurrency PurchaseCurrency { get; set; }
    public CompanyDto SupportCompany { get; set; }
   public int FamilyId { get; set; }
    public string FamilyName { get; set; }
    
    public int TypeId { get; set; }
    public string TypeName { get; set; }
    
    public int BrandId { get; set; }
    public string BrandName { get; set; }
    
    public int ModelId { get; set; }
    public string ModelName { get; set; }
    public string Location { get; set; }
    public string Status { get; set; }
    public string Room { get; set; }
    public string Floor { get; set; }
    public string Block { get; set; }
    public string Department { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string Supplier { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    
    public ICollection<InventoryHistoryDto> InventoryHistory { get; set; }
    public ICollection<InventoryAttachmentDto> Attachments { get; set; }
}