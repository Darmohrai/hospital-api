using hospital_api.DTOs.Patient;
using hospital_api.DTOs.Reports;
using hospital_api.Models.PatientAggregate;

namespace hospital_api.Services.Interfaces;

public interface IPatientService
{
    Task<Patient?> GetByIdAsync(int id);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);

    Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName);
    Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status);
    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId);
    Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId);
    Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId);
    Task<IEnumerable<Patient>> GetAllWithAssociationsAsync();
    
    Task AssignPatientToBedAsync(int patientId, int bedId);

    Task UnassignPatientFromBedAsync(int patientId);
    
    Task<IEnumerable<PatientDetailsDto>> GetPatientListAsync(int hospitalId, int? departmentId, int? roomId);
    
    Task<PatientHistoryDto> GetPatientHistoryAsync(int patientId);
    
    Task AssignDoctorAsync(int patientId, int doctorId);
    Task RemoveDoctorAsync(int patientId);
}
