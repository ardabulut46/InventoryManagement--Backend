using InventoryManagement.Core.DTOs.Company;
using InventoryManagement.Core.DTOs.User;

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
    public CompanyDto SupportCompany { get; set; }
    public string Family { get; set; }
    public string Type { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
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
}