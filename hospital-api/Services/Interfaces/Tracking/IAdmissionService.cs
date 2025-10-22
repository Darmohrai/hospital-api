using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;

namespace hospital_api.Services.Interfaces.Tracking;

public interface IAdmissionService
{
    Task<Admission?> GetByIdAsync(int id);
    Task<IEnumerable<Admission>> GetAllAsync();
    
    /// <summary>
    /// Госпіталізує пацієнта.
    /// </summary>
    Task<Admission> CreateAsync(CreateAdmissionDto dto);
    
    /// <summary>
    /// Виписує пацієнта (встановлює DischargeDate).
    /// </summary>
    Task<Admission?> DischargeAsync(int admissionId, DateTime dischargeDate);
    
    Task<bool> DeleteAsync(int id);
}