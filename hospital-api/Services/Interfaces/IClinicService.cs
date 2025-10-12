using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
public interface IClinicService
{
    Task<IEnumerable<Clinic>> GetAllAsync();
    Task<Clinic?> GetByIdAsync(int id);
    Task CreateAsync(Clinic clinic);
    Task UpdateAsync(Clinic clinic);
    Task DeleteAsync(int id);

    /// <summary>
    /// Призначає клініку до вказаної лікарні.
    /// </summary>
    Task<ServiceResponse<Clinic>> AssignHospitalAsync(int clinicId, int hospitalId);

    /// <summary>
    /// Працевлаштовує існуючого співробітника в клініку.
    /// </summary>
    Task<ServiceResponse<Employment>> AddStaffToClinicAsync(int clinicId, int staffId);
        
    /// <summary>
    /// Додає нового пацієнта до клініки.
    /// </summary>
    Task<ServiceResponse<Patient>> AddPatientAsync(int clinicId, Patient patient);

    /// <summary>
    /// Направляє пацієнта з клініки до лікарні з потрібною спеціалізацією.
    /// </summary>
    Task<ServiceResponse<Hospital>> ReferPatientToHospitalAsync(int patientId, HospitalSpecialization requiredSpecialization);
}