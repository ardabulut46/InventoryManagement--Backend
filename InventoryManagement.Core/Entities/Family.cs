using System;
using System.Collections.Generic;

namespace InventoryManagement.Core.Entities;

public class Family : BaseEntity
{
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Inventory> Inventories { get; set; }
}