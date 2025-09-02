using hospital_api.Models.PatientAggregate;

namespace hospital_api.Repositories.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    // Отримати пацієнтів за повним ім'ям
    Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName);
    
    // Отримати пацієнтів за станом здоров'я
    Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status);

    // Отримати пацієнтів за ID клініки, включаючи клініку
    Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId);

    // Отримати пацієнтів за ID лікарні, включаючи лікарню
    Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId);

    // Отримати пацієнтів, призначених до конкретного лікаря
    Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId);
    
    // Отримати всіх пацієнтів з їхніми пов'язаними сутностями (клінікою, лікарнею, лікарем)
    Task<IEnumerable<Patient>> GetAllWithAssociationsAsync();
}