using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace InventoryManagement.Core.Entities;

public class Permission : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}