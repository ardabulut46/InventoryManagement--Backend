namespace InventoryManagement.Core.Entities;

public class ProblemType : BaseEntity
{
    public string Name { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<SolutionTime> SolutionTime { get; set; } = new List<SolutionTime>();
    public ICollection<AssignmentTime> AssignmentTimes { get; set; } = new List<AssignmentTime>();
}