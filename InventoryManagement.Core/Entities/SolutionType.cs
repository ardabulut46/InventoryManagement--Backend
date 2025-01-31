using System.Runtime;

namespace InventoryManagement.Core.Entities;

public class SolutionType : BaseEntity
{
    public string Name { get; set; }

    public bool IsActive { get; set; } = true;
}