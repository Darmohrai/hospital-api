using hospital_api.Models.StaffAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IStaffRepository : IRepository<Staff>
{
    Task<IEnumerable<SupportStaff>> GetSupportStaffByRoleAsync(SupportRole role);

    Task<IEnumerable<SupportStaff>> GetSupportStaffByClinicAsync(int clinicId, SupportRole? role = null);

    Task<IEnumerable<SupportStaff>> GetSupportStaffByHospitalAsync(int hospitalId, SupportRole? role = null);

    Task<int> GetExtendedVacationDaysForDoctor(int doctorId);

    Task<float> GetHazardPayCoefficientForDoctor(int doctorId);
}