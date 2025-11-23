using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface ISupportStaffService
{
    Task<IEnumerable<SupportStaff>> GetAllAsync();
    Task<SupportStaff?> GetByIdAsync(int id);
    Task CreateAsync(SupportStaff staff, int? hospitalId, int? clinicId);
    Task UpdateAsync(SupportStaff staffFromRequest, int? hospitalId, int? clinicId);
    Task DeleteAsync(int id);
        
    Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role);
    Task<IEnumerable<SupportStaff>> GetByClinicAsync(int clinicId, SupportRole? role = null);
    Task<IEnumerable<SupportStaff>> GetByHospitalAsync(int hospitalId, SupportRole? role = null);
        
    Task<string> GetProfileSummaryAsync(int staffId);
}