using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Services.Interfaces.HospitalServices;

public interface IRoomService
{
    // CRUD
    Task<Room?> GetByIdAsync(int id);
    Task<IEnumerable<Room>> GetAllAsync();
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);

    // Специфічні методи
    Task<Room?> GetByNumberAsync(string roomNumber);
    Task<IEnumerable<Room>> GetByDepartmentIdAsync(int departmentId);
    Task<IEnumerable<Room>> GetByCapacityAsync(int capacity);
    Task<IEnumerable<Room>> GetAllWithBedsAndDepartmentAsync();
}
