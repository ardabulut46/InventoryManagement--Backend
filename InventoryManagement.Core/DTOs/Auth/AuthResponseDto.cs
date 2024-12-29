using InventoryManagement.Core.DTOs.User;

namespace InventoryManagement.Core.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; }
    public UserDto User { get; set; }
}