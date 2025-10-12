using hospital_api.Models.StaffAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IStaffRepository : IRepository<Staff>
{
    /// <summary>
    /// Отримує співробітників типу SupportStaff за їх роллю.
    /// </summary>
    Task<IEnumerable<SupportStaff>> GetSupportStaffByRoleAsync(SupportRole role);

    /// <summary>
    /// Отримує співробітників типу SupportStaff, які працюють у вказаній клініці.
    /// </summary>
    Task<IEnumerable<SupportStaff>> GetSupportStaffByClinicAsync(int clinicId, SupportRole? role = null);

    /// <summary>
    /// Отримує співробітників типу SupportStaff, які працюють у вказаній лікарні.
    /// </summary>
    Task<IEnumerable<SupportStaff>> GetSupportStaffByHospitalAsync(int hospitalId, SupportRole? role = null);
}