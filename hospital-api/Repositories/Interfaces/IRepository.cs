using System.Linq.Expressions;

namespace hospital_api.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);

    /// <summary>
    /// Повертає IQueryable<T> для побудови складних запитів до БД.
    /// Це дозволяє додавати .Include(), .Where() і т.д. у сервісному шарі.
    /// </summary>
    /// <returns>IQueryable колекцію сутностей.</returns>
    IQueryable<T> GetAll(); // ✅ ОСЬ ЦЕЙ РЯДОК ВИРІШУЄ ПРОБЛЕМУ
    
    Task<IEnumerable<T>> FindByConditionAsync(Expression<Func<T, bool>> expression);

}
