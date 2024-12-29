using InventoryManagement.Core.Entities;

namespace InventoryManagement.Core.Services;

public interface ITokenService
{
    Task<string> CreateToken(User user);
}


