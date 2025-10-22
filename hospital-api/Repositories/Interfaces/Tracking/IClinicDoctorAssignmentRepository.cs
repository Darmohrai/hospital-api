using hospital_api.Models.Tracking;

namespace hospital_api.Repositories.Interfaces.Tracking;

public interface IClinicDoctorAssignmentRepository : IRepository<ClinicDoctorAssignment>
{
    Task AddAssignmentAsync(ClinicDoctorAssignment assignment);
    Task RemoveAssignmentAsync(int patientId, int doctorId, int clinicId);
    Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForPatientAsync(int patientId);
    Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForDoctorAsync(int doctorId);
}