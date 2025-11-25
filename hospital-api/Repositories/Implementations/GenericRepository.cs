using System.Linq.Expressions;
using hospital_api.Data;
using hospital_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public IQueryable<T> GetAll()
    {
        return _context.Set<T>().AsQueryable();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        var entityId = (int)entity.GetType().GetProperty("Id").GetValue(entity, null);

        var local = _context.Set<T>()
            .Local
            .FirstOrDefault(entry => 
                (int)entry.GetType().GetProperty("Id").GetValue(entry, null) == entityId);

        if (local != null)
        {
            _context.Entry(local).State = EntityState.Detached;
        }

        _context.Entry(entity).State = EntityState.Modified;
    
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().Where(expression).ToListAsync();
    }
}
