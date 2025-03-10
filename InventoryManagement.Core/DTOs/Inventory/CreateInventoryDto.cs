using System.ComponentModel.DataAnnotations;
using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.DTOs.Inventory;

public class CreateInventoryDto
{
    [Required(ErrorMessage = "Barkod alanı zorunludur")]
    public string Barcode { get; set; }
    
    [Required(ErrorMessage = "Seri numarası zorunludur")]
    public string SerialNumber { get; set; }
    
    [Required(ErrorMessage = "Aile bilgisi zorunludur")]
    public int FamilyId { get; set; }
    
    [Required(ErrorMessage = "Tip bilgisi zorunludur")]
    public int TypeId { get; set; }
    
    [Required(ErrorMessage = "Marka bilgisi zorunludur")]
    public int BrandId { get; set; }
    
    [Required(ErrorMessage = "Model bilgisi zorunludur")]
    public int ModelId { get; set; }
    
    public string? Location { get; set; }
    public string? Status { get; set; }
    public string? Room { get; set; }
    public string? Floor { get; set; }
    public string? Department { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int PurchasePrice { get; set; }
    public PurchaseCurrency PurchaseCurrency { get; set; }
    
    public string? InvoiceAttachmentPath { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string Supplier { get; set; }

    public int? CreatedUserId { get; set; }
    public int? AssignedUserId { get; set; }
    public int? SupportCompanyId { get; set; }
}