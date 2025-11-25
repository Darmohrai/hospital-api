using hospital_api.Data;
using hospital_api.DTOs.Patient;
using hospital_api.DTOs.Reports;
using hospital_api.Models.PatientAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IBedRepository _bedRepository;

    private readonly IAppointmentRepository _appointmentRepo;
    private readonly ILabAnalysisRepository _labAnalysisRepo;
    private readonly IOperationRepository _operationRepo;
    private readonly IAdmissionRepository _admissionRepo;

    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    private readonly ApplicationDbContext _context;

    public PatientService(
        IPatientRepository patientRepository,
        IBedRepository bedRepository,
        IAppointmentRepository appointmentRepo,
        ILabAnalysisRepository labAnalysisRepo,
        IOperationRepository operationRepo,
        IAdmissionRepository admissionRepo,
        IStaffRepository staffRepository,
        IEmploymentRepository employmentRepository,
        ApplicationDbContext context)
    {
        _patientRepository = patientRepository;
        _bedRepository = bedRepository;
        _appointmentRepo = appointmentRepo;
        _labAnalysisRepo = labAnalysisRepo;
        _operationRepo = operationRepo;
        _admissionRepo = admissionRepo;
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _patientRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _patientRepository.GetAllAsync();
    }

    public async Task AddAsync(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
        {
            throw new ArgumentException("Patient full name is required.");
        }

        await _patientRepository.AddAsync(patient);
    }

    public async Task UpdateAsync(Patient patient)
    {
        try
        {
            var existingPatient = await _patientRepository.GetByIdAsync(patient.Id);

            if (existingPatient == null)
            {
                throw new KeyNotFoundException($"Пацієнта з Id = {patient.Id} не знайдено.");
            }

            existingPatient.FullName = patient.FullName;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.HealthStatus = patient.HealthStatus;
            existingPatient.Temperature = patient.Temperature;
            existingPatient.ClinicId = patient.ClinicId;
            existingPatient.HospitalId = patient.HospitalId;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        await _patientRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName)
    {
        return await _patientRepository.GetByFullNameAsync(fullName);
    }

    public async Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status)
    {
        return await _patientRepository.GetByHealthStatusAsync(status);
    }

    public async Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId)
    {
        return await _patientRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _patientRepository.GetByHospitalIdAsync(hospitalId);
    }

    public async Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId)
    {
        return await _patientRepository.GetByAssignedDoctorIdAsync(doctorId);
    }

    public async Task<IEnumerable<Patient>> GetAllWithAssociationsAsync()
    {
        return await _patientRepository.GetAllWithAssociationsAsync();
    }

    public async Task AssignPatientToBedAsync(int patientId, int bedId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        var bed = await _bedRepository.GetByIdAsync(bedId);
        if (bed == null)
            throw new KeyNotFoundException("Bed not found.");

        if (bed.IsOccupied || bed.PatientId != null)
            throw new InvalidOperationException("This bed is already occupied.");

        var existingBed = (await _bedRepository.FindByConditionAsync(b => b.PatientId == patientId)).FirstOrDefault();
        if (existingBed != null)
            throw new InvalidOperationException(
                $"Patient is already assigned to bed {existingBed.Id}. Please unassign first.");

        bed.PatientId = patientId;
        bed.IsOccupied = true;

        await _bedRepository.UpdateAsync(bed);
    }

    public async Task UnassignPatientFromBedAsync(int patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        var bed = (await _bedRepository.FindByConditionAsync(b => b.PatientId == patientId)).FirstOrDefault();

        if (bed == null)
            throw new InvalidOperationException("Patient is not assigned to any bed.");

        bed.PatientId = null;
        bed.IsOccupied = false;

        await _bedRepository.UpdateAsync(bed);
    }

    public async Task<IEnumerable<PatientDetailsDto>> GetPatientListAsync(int hospitalId, int? departmentId,
        int? roomId)
    {
        var allPatients = await _patientRepository.GetAllWithAssociationsAsync();

        var filteredPatients = allPatients
            .Where(p => p.HospitalId == hospitalId);

        if (departmentId.HasValue)
        {
            filteredPatients = filteredPatients
                .Where(p => p.Bed != null && p.Bed.Room.DepartmentId == departmentId.Value);
        }

        if (roomId.HasValue)
        {
            filteredPatients = filteredPatients
                .Where(p => p.Bed != null && p.Bed.RoomId == roomId.Value);
        }

        var resultDto = filteredPatients.Select(p => new PatientDetailsDto
        {
            Id = p.Id,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            HealthStatus = p.HealthStatus,
            Temperature = p.Temperature,

            AttendingDoctorId = p.AssignedDoctorId,
            AttendingDoctorName = p.AssignedDoctor?.FullName ?? "N/A",

            BedId = p.Bed?.Id,
            RoomNumber = p.Bed?.Room?.Number ?? "N/A",
            DepartmentName = p.Bed?.Room?.Department?.Name ?? "N/A"
        });

        return resultDto;
    }

    public async Task<PatientHistoryDto> GetPatientHistoryAsync(int patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        var history = new PatientHistoryDto
        {
            PatientId = patient.Id,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            CurrentHealthStatus = patient.HealthStatus
        };

        var allEvents = new List<PatientHistoryEventDto>();

        var appointments = await _appointmentRepo.GetAllWithAssociationsAsync();
        allEvents.AddRange(appointments
            .Where(a => a.PatientId == patientId)
            .Select(a => new PatientHistoryEventDto
            {
                EventDate = a.VisitDateTime,
                EventType = "Візит до лікаря",
                Description = $"Візит. {a.Summary}",
                DoctorName = a.Doctor?.FullName ?? "N/A",
                LocationName = a.Clinic?.Name ?? a.Hospital?.Name ?? "N/A"
            }));

        var admissions = await _admissionRepo.GetAllWithAssociationsAsync();
        allEvents.AddRange(admissions
            .Where(a => a.PatientId == patientId)
            .Select(a => new PatientHistoryEventDto
            {
                EventDate = a.AdmissionDate,
                EventType = "Госпіталізація",
                Description = $"Початок госпіталізації. Лікар: {a.AttendingDoctor?.FullName ?? "N/A"}",
                DoctorName = a.AttendingDoctor?.FullName ?? "N/A",
                LocationName = a.Hospital?.Name ?? "N/A"
            }));
        allEvents.AddRange(admissions
            .Where(a => a.PatientId == patientId && a.DischargeDate.HasValue)
            .Select(a => new PatientHistoryEventDto
            {
                EventDate = a.DischargeDate.Value,
                EventType = "Виписка",
                Description = "Виписка зі стаціонару.",
                DoctorName = a.AttendingDoctor?.FullName ?? "N/A",
                LocationName = a.Hospital?.Name ?? "N/A"
            }));

        var operations = await _operationRepo.GetAllWithAssociationsAsync();
        allEvents.AddRange(operations
            .Where(op => op.PatientId == patientId)
            .Select(op => new PatientHistoryEventDto
            {
                EventDate = op.Date,
                EventType = "Операція",
                Description = $"Операція: {op.Type}. {(op.IsFatal ? "Результат: Летальний." : "")}",
                DoctorName = op.Doctor?.FullName ?? "N/A",
                LocationName = op.Hospital?.Name ?? op.Clinic?.Name ?? "N/A"
            }));

        var analyses = await _labAnalysisRepo.GetAllWithAssociationsAsync();
        allEvents.AddRange(analyses
            .Where(a => a.PatientId == patientId)
            .Select(a => new PatientHistoryEventDto
            {
                EventDate = a.AnalysisDate,
                EventType = "Лабораторний аналіз",
                Description = $"Аналіз: {a.AnalysisType}. Результат: {a.ResultSummary}",
                DoctorName = "N/A",
                LocationName = a.Laboratory?.Name ?? "N/A"
            }));

        history.Events = allEvents.OrderByDescending(e => e.EventDate).ToList();
        return history;
    }

    public async Task AssignDoctorAsync(int patientId, int doctorId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        var doctor = await _staffRepository.GetByIdAsync(doctorId);
        if (doctor == null || !(doctor is Doctor))
            throw new KeyNotFoundException("Doctor not found.");

        var doctorEmployments = await _employmentRepository.GetEmploymentsByStaffIdAsync(doctorId);

        bool sharesLocation = false;

        if (doctorEmployments.Any(e => e.ClinicId == patient.ClinicId))
        {
            sharesLocation = true;
        }

        if (!sharesLocation && patient.HospitalId.HasValue)
        {
            if (doctorEmployments.Any(e => e.HospitalId == patient.HospitalId.Value))
            {
                sharesLocation = true;
            }
        }

        if (!sharesLocation)
        {
            throw new InvalidOperationException("Doctor and patient do not share the same clinic or hospital.");
        }

        patient.AssignedDoctorId = doctorId;
        await _patientRepository.UpdateAsync(patient);
    }

    public async Task RemoveDoctorAsync(int patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        patient.AssignedDoctorId = null;
        await _patientRepository.UpdateAsync(patient);
    }
}