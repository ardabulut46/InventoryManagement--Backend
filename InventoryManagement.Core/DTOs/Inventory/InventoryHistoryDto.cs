using InventoryManagement.Core.DTOs.User;

namespace InventoryManagement.Core.DTOs.Inventory;

public class InventoryHistoryDto
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public int UserId { get; set; }
    public UserDto User { get; set; }  
    public DateTime AssignmentDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}