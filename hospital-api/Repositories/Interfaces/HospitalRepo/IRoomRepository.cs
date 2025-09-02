using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IRoomRepository : IRepository<Room>
{
    // Отримати кімнату за її номером
    Task<Room?> GetByNumberAsync(string roomNumber);

    // Отримати всі кімнати, що належать певному відділенню
    Task<IEnumerable<Room>> GetByDepartmentIdAsync(int departmentId);
    
    // Отримати кімнати за певною місткістю
    Task<IEnumerable<Room>> GetByCapacityAsync(int capacity);

    // Отримати всі кімнати разом з їхніми ліжками та відділенням
    Task<IEnumerable<Room>> GetAllWithBedsAndDepartmentAsync();
}