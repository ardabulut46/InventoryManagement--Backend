using System;
using System.Threading.Tasks;

namespace InventoryManagement.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync(); // Optional: if you want to explicitly call SaveChanges through UoW
    
    // If your repositories are also managed by UoW, you might have methods like:
    // IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class; 
} 