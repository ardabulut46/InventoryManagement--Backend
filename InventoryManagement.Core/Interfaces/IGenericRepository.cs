using System.Linq.Expressions;
using InventoryManagement.Core.Entities;

namespace InventoryManagement.Core.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync(
        Func<IQueryable<T>, IQueryable<T>> include = null);
    Task<T> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task AddInventoryHistoryAsync(int inventoryId, int userId, string notes = null);
    
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includes);
    Task<T> GetByIdWithIncludesAsync(int id, params string[] includes);
    
    Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> expression);
    Task<IEnumerable<T>> SearchWithIncludesAsync(Expression<Func<T,bool>> expression, params string[] includes);
}