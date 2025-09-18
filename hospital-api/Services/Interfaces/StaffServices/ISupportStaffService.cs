using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface ISupportStaffService
{
    Task<IEnumerable<SupportStaff>> GetAllSupportStaffAsync();
    Task<SupportStaff?> GetSupportStaffByIdAsync(int id);
    Task AddSupportStaffAsync(SupportStaff staff);
    Task UpdateSupportStaffAsync(SupportStaff staff);
    Task DeleteSupportStaffAsync(int id);

    Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role);
    Task<IEnumerable<SupportStaff>> GetByClinicIdAndRoleAsync(int clinicId, SupportRole role);
    Task<IEnumerable<SupportStaff>> GetByHospitalIdAndRoleAsync(int hospitalId, SupportRole role);

    Task<string> GetSupportStaffProfileSummaryAsync(int staffId);
}
