using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Interfaces.Tracking;

namespace hospital_api.Services.Implementations.Tracking;

public class ClinicDoctorAssignmentService : IClinicDoctorAssignmentService
{
    private readonly IClinicDoctorAssignmentRepository _assignmentRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly IStaffRepository _staffRepo;
    private readonly IClinicRepository _clinicRepo;

    public ClinicDoctorAssignmentService(
        IClinicDoctorAssignmentRepository assignmentRepo,
        IPatientRepository patientRepo,
        IStaffRepository staffRepo,
        IClinicRepository clinicRepo)
    {
        _assignmentRepo = assignmentRepo;
        _patientRepo = patientRepo;
        _staffRepo = staffRepo;
        _clinicRepo = clinicRepo;
    }

    public Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForPatientAsync(int patientId)
    {
        return _assignmentRepo.GetAssignmentsForPatientAsync(patientId);
    }

    public Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForDoctorAsync(int doctorId)
    {
        return _assignmentRepo.GetAssignmentsForDoctorAsync(doctorId);
    }

    public async Task<ClinicDoctorAssignment> CreateAsync(ClinicDoctorAssignmentDto dto)
    {
        if (await _patientRepo.GetByIdAsync(dto.PatientId) == null)
            throw new KeyNotFoundException("Пацієнта з таким ID не знайдено.");
        
        if (await _staffRepo.GetByIdAsync(dto.DoctorId) == null) // TODO: Перевірити, що це саме Doctor
            throw new KeyNotFoundException("Лікаря з таким ID не знайдено.");
        
        if (await _clinicRepo.GetByIdAsync(dto.ClinicId) == null)
            throw new KeyNotFoundException("Клініку з таким ID не знайдено.");

        var existing = (await _assignmentRepo.FindByConditionAsync(
            a => a.PatientId == dto.PatientId &&
                 a.DoctorId == dto.DoctorId &&
                 a.ClinicId == dto.ClinicId
        )).Any();
        
        if (existing)
            throw new InvalidOperationException("Таке призначення вже існує.");

        var assignment = new ClinicDoctorAssignment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            ClinicId = dto.ClinicId
        };
        
        await _assignmentRepo.AddAssignmentAsync(assignment);
        return assignment;
    }

    public async Task<bool> DeleteAsync(int patientId, int doctorId, int clinicId)
    {
        await _assignmentRepo.RemoveAssignmentAsync(patientId, doctorId, clinicId);
        
        return true; 
    }
}