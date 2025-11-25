using hospital_api.DTOs.Clinic;
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

    Task<ServiceResponse<Clinic>> AssignHospitalAsync(int clinicId, int hospitalId);

    Task<ServiceResponse<Employment>> AddStaffToClinicAsync(int clinicId, int staffId);
    
    Task<ServiceResponse<Patient>> AddPatientAsync(int clinicId, Patient patient);

    Task<ServiceResponse<Hospital>> ReferPatientToHospitalAsync(int patientId, HospitalSpecialization requiredSpecialization);
    
    Task<IEnumerable<ClinicDto>> GetAllDtosAsync();
}