using System.Linq.Expressions;

namespace hospital_api.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);

    IQueryable<T> GetAll();
    
    Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression);

}
