namespace InventoryManagement.Core.Entities;

public class ProblemType : BaseEntity
{
    public string Name { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; } // group'a güncellenecek
    public bool IsActive { get; set; } = true;
    
    public ICollection<SolutionTime> SolutionTime { get; set; }
}