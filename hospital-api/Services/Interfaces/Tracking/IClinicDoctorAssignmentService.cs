using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;

namespace hospital_api.Services.Interfaces.Tracking;

public interface IClinicDoctorAssignmentService
{
    Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForPatientAsync(int patientId);
    
    Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForDoctorAsync(int doctorId);

    Task<ClinicDoctorAssignment> CreateAsync(ClinicDoctorAssignmentDto dto);

    Task<bool> DeleteAsync(int patientId, int doctorId, int clinicId);
}