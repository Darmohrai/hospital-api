using hospital_api.Models.ClinicAggregate;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;

public interface IClinicService
{
    Task<IEnumerable<Clinic>> GetAllClinicsAsync();
    Task<Clinic?> GetClinicByIdAsync(int id);
    Task<Clinic> CreateClinicAsync(Clinic clinic);
    Task<Clinic> UpdateClinicAsync(Clinic clinic);
    Task DeleteClinicAsync(int id);

    // Бізнес-логіка
    Task AssignHospitalAsync(int clinicId, int hospitalId);
    Task AddStaffToClinicAsync(int clinicId, Staff staff);
    Task AddPatientAsync(int clinicId, Patient patient);

    /// <summary>
    /// Направити пацієнта в лікарню:
    /// 1. Якщо "рідна" лікарня підходить за спеціалізацією → туди
    /// 2. Якщо ні → шукаємо інший заклад з потрібною спеціалізацією
    /// </summary>
    public Task<Hospital?> ReferPatientToHospitalAsync(int patientId, HospitalSpecialization requiredSpecialization);
}
