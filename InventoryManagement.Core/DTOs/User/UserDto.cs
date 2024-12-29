namespace InventoryManagement.Core.DTOs.User;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Location { get; set; }
    public string Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}