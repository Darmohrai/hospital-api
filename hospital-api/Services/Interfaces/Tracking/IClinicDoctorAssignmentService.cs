using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;

namespace hospital_api.Services.Interfaces.Tracking;

public interface IClinicDoctorAssignmentService
{
    /// <summary>
    /// Отримує всі призначення (лікарів) для одного пацієнта.
    /// </summary>
    Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForPatientAsync(int patientId);
    
    /// <summary>
    /// Отримує всі призначення (пацієнтів) для одного лікаря.
    /// </summary>
    Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForDoctorAsync(int doctorId);

    /// <summary>
    /// Створює новий зв'язок (призначає лікаря пацієнту в клініці).
    /// </summary>
    Task<ClinicDoctorAssignment> CreateAsync(ClinicDoctorAssignmentDto dto);

    /// <summary>
    /// Видаляє зв'язок.
    /// </summary>
    Task<bool> DeleteAsync(int patientId, int doctorId, int clinicId);
}