using System.Linq.Expressions;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    
    
    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<T>> GetAllAsync(
        Func<IQueryable<T>, IQueryable<T>> include = null)
    {
        IQueryable<T> query = _context.Set<T>();
    
        if (include != null)
        {
            query = include(query);
        }
    
        return await query.ToListAsync();
    }
    
    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.UpdatedDate = DateTime.Now;
        }
        
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
    public async Task AddInventoryHistoryAsync(int inventoryId, int userId, string notes = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception($"User with ID {userId} not found");
        }

        var history = new InventoryHistory
        {
            InventoryId = inventoryId,
            UserId = userId,
            AssignmentDate = DateTime.Now,
            Notes = notes
        };

        await _context.InventoryHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includes)
    {
        var query = _context.Set<T>().AsQueryable();
    
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<T> GetByIdWithIncludesAsync(int id, params string[] includes)
    {
        var query = _context.Set<T>().AsQueryable();
    
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<T>> SearchAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().Where(expression).ToListAsync();
    }

    public async Task<IEnumerable<T>> SearchWithIncludesAsync(Expression<Func<T, bool>> expression, params string[] includes)
    {
        var query = _context.Set<T>().AsQueryable();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return await query.Where(expression).ToListAsync();
        
    }
    
    
}