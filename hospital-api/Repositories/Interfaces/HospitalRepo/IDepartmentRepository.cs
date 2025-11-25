using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<Department?> GetByNameAsync(string name);
    
    Task<IEnumerable<Department>> GetBySpecializationAsync(string specialization);

    Task<IEnumerable<Department>> GetByBuildingIdWithRoomsAsync(int buildingId);
}