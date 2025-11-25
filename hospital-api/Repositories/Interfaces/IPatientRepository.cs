using hospital_api.Models.PatientAggregate;

namespace hospital_api.Repositories.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName);
    
    Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status);

    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId);

    Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId);

    Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId);
    
    Task<IEnumerable<Patient>> GetAllWithAssociationsAsync();
    
}