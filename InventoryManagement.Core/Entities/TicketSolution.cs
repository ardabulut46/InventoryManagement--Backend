namespace InventoryManagement.Core.Entities;

public class TicketSolution : BaseEntity
{
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int AssignedUserId { get; set; }
    public User AssignedUser { get; set; }
    
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int SolutionTypeId { get; set; }
    public SolutionType SolutionType { get; set; }

    public DateTime SolutionDate { get; set; } = DateTime.Now;

    public bool? IsChronicle { get; set; }
    public string? AttachmentPath { get; set; }

    public virtual SolutionReview? SolutionReview { get; set; }      
    
    public virtual ICollection<TicketSolutionAttachment> Attachments { get; set; } = new List<TicketSolutionAttachment>();
}