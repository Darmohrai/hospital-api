using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IStaffService
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<ServiceResponse<Staff>> CreateAsync(Staff staff);
    Task<ServiceResponse<Staff>> UpdateAsync(Staff staff);
    Task<ServiceResponse<bool>> DeleteAsync(int id);

    /// <summary>
    /// Отримує список співробітників, що працюють у вказаній клініці.
    /// </summary>
    Task<IEnumerable<Staff>> GetByClinicAsync(int clinicId);

    /// <summary>
    /// Отримує список співробітників, що працюють у вказаній лікарні.
    /// </summary>
    Task<IEnumerable<Staff>> GetByHospitalAsync(int hospitalId);

    /// <summary>
    /// Отримує список співробітників з досвідом роботи не менше вказаного.
    /// </summary>
    Task<IEnumerable<Staff>> GetExperiencedStaffAsync(int minExperienceYears);
        
    /// <summary>
    /// Розраховує річний бонус для співробітника на основі бізнес-правил.
    /// </summary>
    Task<ServiceResponse<decimal>> CalculateAnnualBonusAsync(int staffId);
}