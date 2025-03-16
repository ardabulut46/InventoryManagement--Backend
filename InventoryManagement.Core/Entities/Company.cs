using InventoryManagement.Core.Enums;

namespace InventoryManagement.Core.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    
    public CompanyType CompanyType { get; set; }
}