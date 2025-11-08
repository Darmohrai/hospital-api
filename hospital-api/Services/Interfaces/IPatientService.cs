using hospital_api.DTOs.Patient;
using hospital_api.DTOs.Reports;
using hospital_api.Models.PatientAggregate;

namespace hospital_api.Services.Interfaces;

public interface IPatientService
{
    // CRUD
    Task<Patient?> GetByIdAsync(int id);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);

    // Специфічні методи
    Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName);
    Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status);
    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId);
    Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId);
    Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId);
    Task<IEnumerable<Patient>> GetAllWithAssociationsAsync();
    
    /// <summary>
    /// Призначає пацієнта на конкретне ліжко.
    /// </summary>
    /// <param name="patientId">ID пацієнта</param>
    /// <param name="bedId">ID ліжка</param>
    Task AssignPatientToBedAsync(int patientId, int bedId);

    /// <summary>
    /// Звільняє ліжко, яке займає пацієнт (наприклад, при виписці).
    /// </summary>
    /// <param name="patientId">ID пацієнта</param>
    Task UnassignPatientFromBedAsync(int patientId);
    
    /// <summary>
    /// (Запит №4) Отримує список пацієнтів з деталями,
    /// відфільтрований за лікарнею, відділенням або палатою.
    /// </summary>
    Task<IEnumerable<PatientDetailsDto>> GetPatientListAsync(int hospitalId, int? departmentId, int? roomId);
    
    Task<PatientHistoryDto> GetPatientHistoryAsync(int patientId);
    
    Task AssignDoctorAsync(int patientId, int doctorId);
    Task RemoveDoctorAsync(int patientId);
}
