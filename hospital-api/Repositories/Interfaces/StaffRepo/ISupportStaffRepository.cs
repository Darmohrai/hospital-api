using hospital_api.Models.StaffAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface ISupportStaffRepository : IRepository<SupportStaff>
{
    // Отримати допоміжний персонал за роллю
    Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role);

    // Отримати допоміжний персонал за ID клініки та роллю
    Task<IEnumerable<SupportStaff>> GetByClinicIdAndRoleAsync(int clinicId, SupportRole role);
    
    // Отримати допоміжний персонал за ID лікарні та роллю
    Task<IEnumerable<SupportStaff>> GetByHospitalIdAndRoleAsync(int hospitalId, SupportRole role);
}