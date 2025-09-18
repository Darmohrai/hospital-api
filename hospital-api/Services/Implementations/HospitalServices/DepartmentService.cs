using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Services.Interfaces.HospitalServices;

namespace hospital_api.Services.Implementations.HospitalServices;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    // --- CRUD ---
    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _departmentRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _departmentRepository.GetAllAsync();
    }

    public async Task AddAsync(Department department)
    {
        await _departmentRepository.AddAsync(department);
    }

    public async Task UpdateAsync(Department department)
    {
        await _departmentRepository.UpdateAsync(department);
    }

    public async Task DeleteAsync(int id)
    {
        await _departmentRepository.DeleteAsync(id);
    }

    // --- Специфічні методи ---
    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _departmentRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<Department>> GetBySpecializationAsync(string specialization)
    {
        return await _departmentRepository.GetBySpecializationAsync(specialization);
    }

    public async Task<IEnumerable<Department>> GetByBuildingIdWithRoomsAsync(int buildingId)
    {
        return await _departmentRepository.GetByBuildingIdWithRoomsAsync(buildingId);
    }
}
