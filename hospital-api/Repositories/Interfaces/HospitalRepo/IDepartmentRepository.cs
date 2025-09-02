using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IDepartmentRepository : IRepository<Department>
{
    // Отримати відділення за назвою
    Task<Department?> GetByNameAsync(string name);
    
    // Отримати відділення за спеціалізацією
    Task<IEnumerable<Department>> GetBySpecializationAsync(string specialization);

    // Отримати всі відділення, що належать певній будівлі, включаючи пов'язані кімнати
    Task<IEnumerable<Department>> GetByBuildingIdWithRoomsAsync(int buildingId);
}