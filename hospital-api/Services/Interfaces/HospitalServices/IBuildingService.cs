using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Services.Interfaces.HospitalServices;

public interface IBuildingService
{
    // CRUD
    Task<Building?> GetByIdAsync(int id);
    Task<IEnumerable<Building>> GetAllAsync();
    Task AddAsync(Building building);
    Task UpdateAsync(Building building);
    Task DeleteAsync(int id);

    // Специфічні методи
    Task<Building?> GetByNameAsync(string name);
    Task<IEnumerable<Building>> GetByHospitalIdAsync(int hospitalId);
    Task<IEnumerable<Building>> GetAllWithDepartmentsAsync();
}
