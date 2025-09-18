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
}
