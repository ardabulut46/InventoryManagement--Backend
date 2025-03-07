using System;
using System.Collections.Generic;

namespace InventoryManagement.Core.Entities;

public class Model : BaseEntity
{
    public string Name { get; set; }
    public int BrandId { get; set; }
    public Brand Brand { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Inventory> Inventories { get; set; }
}