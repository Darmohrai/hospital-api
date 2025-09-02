using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IBuildingRepository : IRepository<Building>
{
    // Отримати будівлю за її назвою
    Task<Building?> GetByNameAsync(string name);
    
    // Отримати всі будівлі, що належать певній лікарні
    Task<IEnumerable<Building>> GetByHospitalIdAsync(int hospitalId);

    // Отримати всі будівлі разом з їхніми відділеннями
    Task<IEnumerable<Building>> GetAllWithDepartmentsAsync();
}