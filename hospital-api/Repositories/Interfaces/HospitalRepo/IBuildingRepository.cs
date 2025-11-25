using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IBuildingRepository : IRepository<Building>
{
    Task<Building?> GetByNameAsync(string name);
    
    Task<IEnumerable<Building>> GetByHospitalIdAsync(int hospitalId);

    Task<IEnumerable<Building>> GetAllWithDepartmentsAsync();
}