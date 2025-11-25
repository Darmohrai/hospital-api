using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<ServiceResponse<Staff>> CreateAsync(Staff staff);
    Task<ServiceResponse<Staff>> UpdateAsync(Staff staff);
    Task<ServiceResponse<bool>> DeleteAsync(int id);

    Task<IEnumerable<Staff>> GetByClinicAsync(int clinicId);

    Task<IEnumerable<Staff>> GetByHospitalAsync(int hospitalId);

    Task<IEnumerable<Staff>> GetExperiencedStaffAsync(int minExperienceYears);
    
    Task<ServiceResponse<decimal>> CalculateAnnualBonusAsync(int staffId);
}