using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Services.Interfaces.HospitalServices;

public interface IDepartmentService
{
    // CRUD
    Task<Department?> GetByIdAsync(int id);
    Task<IEnumerable<Department>> GetAllAsync();
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(int id);

    // Специфічні методи
    Task<Department?> GetByNameAsync(string name);
    Task<IEnumerable<Department>> GetBySpecializationAsync(string specialization);
    Task<IEnumerable<Department>> GetByBuildingIdWithRoomsAsync(int buildingId);
}
