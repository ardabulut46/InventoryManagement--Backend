using InventoryManagement.Core.Entities;

namespace InventoryManagement.Core.Interfaces;

public interface IPermissionRepository : IGenericRepository<Permission>
{
    
    Task<Permission> GetByUserIdAsync(int userId);
}