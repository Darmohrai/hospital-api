using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Services.Interfaces.HospitalServices;

namespace hospital_api.Services.Implementations.HospitalServices;

public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _buildingRepository;

    public BuildingService(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async Task<Building?> GetByIdAsync(int id)
    {
        return await _buildingRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Building>> GetAllAsync()
    {
        return await _buildingRepository.GetAllAsync();
    }

    public async Task AddAsync(Building building)
    {
        building.Hospital = null;
        await _buildingRepository.AddAsync(building);
    }

    public async Task UpdateAsync(Building building)
    {
        await _buildingRepository.UpdateAsync(building);
    }

    public async Task DeleteAsync(int id)
    {
        await _buildingRepository.DeleteAsync(id);
    }

    public async Task<Building?> GetByNameAsync(string name)
    {
        return await _buildingRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<Building>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _buildingRepository.GetByHospitalIdAsync(hospitalId);
    }

    public async Task<IEnumerable<Building>> GetAllWithDepartmentsAsync()
    {
        return await _buildingRepository.GetAllWithDepartmentsAsync();
    }
}
