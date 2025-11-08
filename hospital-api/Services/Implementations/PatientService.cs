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

    // ✅ ДОДАЙ НОВІ РЕПОЗИТОРІЇ
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly ILabAnalysisRepository _labAnalysisRepo;
    private readonly IOperationRepository _operationRepo;
    private readonly IAdmissionRepository _admissionRepo;

    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    private readonly ApplicationDbContext _context;

    // ✅ ОНОВІТЬ КОНСТРУКТОР
    public PatientService(
        IPatientRepository patientRepository,
        IBedRepository bedRepository,
        IAppointmentRepository appointmentRepo,
        ILabAnalysisRepository labAnalysisRepo,
        IOperationRepository operationRepo,
        IAdmissionRepository admissionRepo,
        IStaffRepository staffRepository, // ✅ Додано
        IEmploymentRepository employmentRepository,
        ApplicationDbContext context) // ✅ Додано
    {
        _patientRepository = patientRepository;
        _bedRepository = bedRepository;
        _appointmentRepo = appointmentRepo;
        _labAnalysisRepo = labAnalysisRepo;
        _operationRepo = operationRepo;
        _admissionRepo = admissionRepo;
        _staffRepository = staffRepository; // ✅ Додано
        _employmentRepository = employmentRepository; // ✅ Додано
        _context = context;
    }

    // --- CRUD ---
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
            // 1. Отримуємо існуючого пацієнта з бази даних.
            //    (Я припускаю, що ваш репозиторій має метод GetByIdAsync,
            //     який відстежує сутність. Якщо ні, використовуйте _context)
            var existingPatient = await _patientRepository.GetByIdAsync(patient.Id);

            if (existingPatient == null)
            {
                throw new KeyNotFoundException($"Пацієнта з Id = {patient.Id} не знайдено.");
            }

            // 2. Копіюємо нові значення зі 'patient' (з запиту) 
            //    в 'existingPatient' (який вже відстежується).
            //    НЕ ВИКОРИСТОВУЙТЕ _context.Update(patient);

            existingPatient.FullName = patient.FullName;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.HealthStatus = patient.HealthStatus;
            existingPatient.Temperature = patient.Temperature;
            existingPatient.ClinicId = patient.ClinicId;
            existingPatient.HospitalId = patient.HospitalId;

            // Важливо: ми НЕ чіпаємо 'AssignedDoctorId' або 'BedId',
            // оскільки вони керуються окремими ендпоінтами (AssignDoctor, AssignBed)
            // і фронтенд (patients.js) видаляє їх з DTO перед відправкою.

            // 3. Зберігаємо зміни. 
            //    Викликаємо SaveChangesAsync() безпосередньо з контексту.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Обробка випадків, коли хтось інший змінив дані одночасно
            throw;
        }
        catch (Exception ex)
        {
            // Додаткова обробка помилок
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        await _patientRepository.DeleteAsync(id);
    }

    // --- Специфічні методи ---
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
        // 1. Знаходимо пацієнта
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        // 2. Знаходимо ліжко
        var bed = await _bedRepository.GetByIdAsync(bedId);
        if (bed == null)
            throw new KeyNotFoundException("Bed not found.");

        // 3. Перевіряємо, чи ліжко вільне
        if (bed.IsOccupied || bed.PatientId != null)
            throw new InvalidOperationException("This bed is already occupied.");

        // 4. Перевіряємо, чи пацієнт вже не лежить на іншому ліжку
        var existingBed = (await _bedRepository.FindByConditionAsync(b => b.PatientId == patientId)).FirstOrDefault();
        if (existingBed != null)
            throw new InvalidOperationException(
                $"Patient is already assigned to bed {existingBed.Id}. Please unassign first.");

        // 5. Призначаємо ліжко
        bed.PatientId = patientId;
        bed.IsOccupied = true;

        await _bedRepository.UpdateAsync(bed);
    }

    // ✅ НОВА РЕАЛІЗАЦІЯ: Звільнення ліжка
    public async Task UnassignPatientFromBedAsync(int patientId)
    {
        // 1. Перевіряємо, чи пацієнт існує (опціонально, але бажано)
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        // 2. Знаходимо ліжко, яке займає цей пацієнт
        var bed = (await _bedRepository.FindByConditionAsync(b => b.PatientId == patientId)).FirstOrDefault();

        // 3. Якщо пацієнт не був на ліжку, нічого не робимо (або кидаємо помилку)
        if (bed == null)
            throw new InvalidOperationException("Patient is not assigned to any bed.");

        // 4. Звільняємо ліжко
        bed.PatientId = null;
        bed.IsOccupied = false;

        await _bedRepository.UpdateAsync(bed);
    }

    public async Task<IEnumerable<PatientDetailsDto>> GetPatientListAsync(int hospitalId, int? departmentId,
        int? roomId)
    {
        // 1. Отримуємо ВСІХ пацієнтів з УСІМА зв'язками (ми оновили цей метод)
        var allPatients = await _patientRepository.GetAllWithAssociationsAsync();

        // 2. Фільтруємо в пам'яті (in-memory) на основі завантажених даних
        var filteredPatients = allPatients
            .Where(p => p.HospitalId == hospitalId); // Обов'язковий фільтр по лікарні

        if (departmentId.HasValue)
        {
            // Фільтр по відділенню
            filteredPatients = filteredPatients
                .Where(p => p.Bed != null && p.Bed.Room.DepartmentId == departmentId.Value);
        }

        if (roomId.HasValue)
        {
            // Фільтр по палаті (вже включає фільтр по відділенню)
            filteredPatients = filteredPatients
                .Where(p => p.Bed != null && p.Bed.RoomId == roomId.Value);
        }

        // 3. Перетворюємо (мапимо) відфільтровані дані в DTO
        var resultDto = filteredPatients.Select(p => new PatientDetailsDto
        {
            Id = p.Id,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            HealthStatus = p.HealthStatus,
            Temperature = p.Temperature,

            // Дані лікаря
            AttendingDoctorId = p.AssignedDoctorId,
            AttendingDoctorName = p.AssignedDoctor?.FullName ?? "N/A",

            // Дані про місцезнаходження (з перевіркою на null)
            BedId = p.Bed?.Id,
            RoomNumber = p.Bed?.Room?.Number ?? "N/A",
            DepartmentName = p.Bed?.Room?.Department?.Name ?? "N/A"
        });

        return resultDto;
    }

    public async Task<PatientHistoryDto> GetPatientHistoryAsync(int patientId)
    {
        // 1. Знаходимо пацієнта
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

        // 2. Збираємо Візити (Appointments)
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

        // 3. Збираємо Госпіталізації (Admissions)
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
        // Додаємо події виписки
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

        // 4. Збираємо Операції (Operations)
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

        // 5. Збираємо Аналізи (LabAnalyses)
        var analyses = await _labAnalysisRepo.GetAllWithAssociationsAsync();
        allEvents.AddRange(analyses
            .Where(a => a.PatientId == patientId)
            .Select(a => new PatientHistoryEventDto
            {
                EventDate = a.AnalysisDate,
                EventType = "Лабораторний аналіз",
                Description = $"Аналіз: {a.AnalysisType}. Результат: {a.ResultSummary}",
                DoctorName = "N/A", // Аналізи не мають прямого лікаря в моделі
                LocationName = a.Laboratory?.Name ?? "N/A"
            }));

        // 6. Сортуємо всі події за датою та повертаємо
        history.Events = allEvents.OrderByDescending(e => e.EventDate).ToList();
        return history;
    }

    /// <summary>
    /// Призначає лікаря пацієнту з перевіркою спільної локації.
    /// </summary>
    public async Task AssignDoctorAsync(int patientId, int doctorId)
    {
        // 1. Отримуємо пацієнта
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        // 2. Отримуємо лікаря (переконуємось, що він існує)
        var doctor = await _staffRepository.GetByIdAsync(doctorId);
        if (doctor == null || !(doctor is Doctor)) // Перевіряємо, що це лікар
            throw new KeyNotFoundException("Doctor not found.");

        // 3. ✅ ВИПРАВЛЕННЯ: Використовуємо метод, який тепер існує
        var doctorEmployments = await _employmentRepository.GetEmploymentsByStaffIdAsync(doctorId);

        // 4. ✅ БІЗНЕС-ЛОГІКА: Перевірка локації
        bool sharesLocation = false;

        // Перевіряємо, чи працює лікар у клініці пацієнта
        if (doctorEmployments.Any(e => e.ClinicId == patient.ClinicId))
        {
            sharesLocation = true;
        }

        // Якщо пацієнт у лікарні (має HospitalId), перевіряємо лікарні
        if (!sharesLocation && patient.HospitalId.HasValue)
        {
            if (doctorEmployments.Any(e => e.HospitalId == patient.HospitalId.Value))
            {
                sharesLocation = true;
            }
        }

        // 5. Якщо локація не збігається, кидаємо помилку
        if (!sharesLocation)
        {
            throw new InvalidOperationException("Doctor and patient do not share the same clinic or hospital.");
        }

        // 6. Призначаємо лікаря
        patient.AssignedDoctorId = doctorId;
        await _patientRepository.UpdateAsync(patient); // Використовуємо репозиторій напряму
    }

    /// <summary>
    /// Відкріплює лікаря від пацієнта.
    /// </summary>
    public async Task RemoveDoctorAsync(int patientId)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found.");

        patient.AssignedDoctorId = null;
        await _patientRepository.UpdateAsync(patient);
    }
}