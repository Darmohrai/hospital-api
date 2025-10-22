using hospital_api.DTOs.Reports;
using hospital_api.Models.StaffAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IStaffRepository _staffRepo; // Потрібен для фільтрації за профілем

    private readonly IBuildingRepository _buildingRepo;

    private readonly IHospitalRepository _hospitalRepo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IBedRepository _bedRepo;

    private readonly ILabAnalysisRepository _labAnalysisRepo;
    private readonly ILaboratoryRepository _laboratoryRepo; // Потрібен для M:M зв'язку

    private readonly IOperationRepository _operationRepo;

    private readonly IClinicDoctorAssignmentRepository _clinicAssignmentRepo;
    private readonly IPatientRepository _patientRepo;

    private readonly IAdmissionRepository _admissionRepo;

    // ✅ ОНОВИ КОНСТРУКТОР
    public ReportService(
        IAppointmentRepository appointmentRepo,
        IStaffRepository staffRepo,
        IHospitalRepository hospitalRepo,
        IBuildingRepository buildingRepo,
        IDepartmentRepository departmentRepo,
        IRoomRepository roomRepo,
        IBedRepository bedRepo,
        ILabAnalysisRepository labAnalysisRepo,
        ILaboratoryRepository laboratoryRepo,
        IOperationRepository operationRepo,
        IClinicDoctorAssignmentRepository clinicAssignmentRepo,
        IPatientRepository patientRepo,
        IAdmissionRepository admissionRepo) // <-- Новий
    {
        _appointmentRepo = appointmentRepo;
        _staffRepo = staffRepo;
        _hospitalRepo = hospitalRepo;
        _buildingRepo = buildingRepo;
        _departmentRepo = departmentRepo;
        _roomRepo = roomRepo;
        _bedRepo = bedRepo;
        _labAnalysisRepo = labAnalysisRepo;
        _laboratoryRepo = laboratoryRepo;
        _operationRepo = operationRepo;
        _clinicAssignmentRepo = clinicAssignmentRepo;
        _patientRepo = patientRepo;
        _admissionRepo = admissionRepo; // <-- Новий
    }

    public async Task<AppointmentCountDto> GetDailyAppointmentCountAsync(DateTime date, int? doctorId, int? clinicId,
        string? specialty)
    {
        // 1. Починаємо з базового запиту до IQueryable
        var query = (await _appointmentRepo.FindByConditionAsync(a => a.VisitDateTime.Date == date.Date))
            .AsQueryable();

        var filterDescription = $"Звіт за {date.ToShortDateString()}";

        // 2. Застосовуємо фільтри

        // Сценарій 1: Конкретний лікар
        if (doctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == doctorId.Value);
            var doctor = await _staffRepo.GetByIdAsync(doctorId.Value);
            filterDescription += $", Лікар: {doctor?.FullName ?? "Невідомий"}";
        }
        // Сценарій 2: Поліклініка
        else if (clinicId.HasValue)
        {
            query = query.Where(a => a.ClinicId == clinicId.Value);
            filterDescription += $", Поліклініка ID: {clinicId.Value}";

            // Сценарій 3: Поліклініка + Профіль
            if (!string.IsNullOrEmpty(specialty))
            {
                // Отримуємо ID всіх лікарів потрібного профілю В ЦІЙ КЛІНІЦІ
                var doctorIdsInClinicWithSpecialty = (await _staffRepo
                        .FindByConditionAsync(s =>
                            s.Employments.Any(e => e.ClinicId == clinicId.Value) && // Працює в цій клініці
                            EF.Property<string>(s, "Specialty") == specialty)) // І має цей профіль
                    .Select(s => s.Id);

                query = query.Where(a => doctorIdsInClinicWithSpecialty.Contains(a.DoctorId));
                filterDescription += $", Профіль: {specialty}";
            }
            else
            {
                filterDescription += ", Всі лікарі";
            }
        }
        // Сценарій 4: Тільки профіль (у всіх закладах)
        else if (!string.IsNullOrEmpty(specialty))
        {
            // Отримуємо ID всіх лікарів потрібного профілю
            var doctorIdsWithSpecialty = (await _staffRepo
                    .FindByConditionAsync(s =>
                        EF.Property<string>(s, "Specialty") == specialty)) // Має цей профіль
                .Select(s => s.Id);

            query = query.Where(a => doctorIdsWithSpecialty.Contains(a.DoctorId));
            filterDescription += $", Профіль (всі заклади): {specialty}";
        }
        else
        {
            filterDescription += ", Всі лікарі, всі заклади";
        }

        // 3. Виконуємо запит та групуємо

        // Нам потрібен повний список для групування
        var appointments = query.ToList();

        var countByDoctor = appointments
            .GroupBy(a => a.DoctorId) // Групуємо по ID лікаря
            .Select(g => new DoctorAppointmentCountDto
            {
                DoctorId = g.Key,
                // Ми не можемо тут дістати FullName без N+1 запитів. 
                // Для простоти залишимо ID, або треба додати Include(a => a.Doctor) в FindByConditionAsync
                DoctorFullName = g.First().Doctor?.FullName ?? $"DoctorID {g.Key}", // Потребує Include()
                PatientCount = g.Count() // Кількість візитів = кількість пацієнтів для цього лікаря
            })
            .ToList();

        // 4. Формуємо DTO
        var result = new AppointmentCountDto
        {
            FilterDescription = filterDescription,
            ReportDate = date.Date,
            PatientCount = appointments.Count, // Загальна кількість
            CountByDoctor = countByDoctor
        };

        return result;
    }

    public async Task<HospitalCapacityReportDto> GetHospitalCapacityReportAsync(int hospitalId)
    {
        var hospital = await _hospitalRepo.GetByIdAsync(hospitalId);
        if (hospital == null)
            throw new KeyNotFoundException("Hospital not found.");

        var report = new HospitalCapacityReportDto
        {
            HospitalId = hospital.Id,
            HospitalName = hospital.Name
        };

        var departmentReports = new List<DepartmentCapacityDto>();

        // 1. Отримуємо всі корпуси (Buildings) лікарні
        var buildings = await _buildingRepo.FindByConditionAsync(b => b.HospitalId == hospitalId);
        var buildingIds = buildings.Select(b => b.Id);

        // 2. Отримуємо всі відділення (Departments) В ЦИХ корпусах
        var departments = await _departmentRepo.FindByConditionAsync(d => buildingIds.Contains(d.BuildingId));

        foreach (var dept in departments)
        {
            // ... (решта логіки не змінилася) ...

            // 3. Для кожного відділення отримуємо всі палати
            var rooms = (await _roomRepo.FindByConditionAsync(r => r.DepartmentId == dept.Id)).ToList();
            var roomIds = rooms.Select(r => r.Id).ToList();

            // 4. Отримуємо всі ліжка в цих палатах
            var beds = (await _bedRepo.FindByConditionAsync(b => roomIds.Contains(b.RoomId))).ToList();

            // 5. Рахуємо статистику для відділення
            var freeBeds = beds.Count(b => !b.IsOccupied);

            int fullyFreeRooms = 0;
            foreach (var room in rooms)
            {
                var bedsInRoom = beds.Where(b => b.RoomId == room.Id);
                if (bedsInRoom.Any() && bedsInRoom.All(b => !b.IsOccupied))
                {
                    fullyFreeRooms++;
                }
            }

            // 6. Додаємо звіт по відділенню
            departmentReports.Add(new DepartmentCapacityDto
            {
                DepartmentId = dept.Id,
                DepartmentName = dept.Name,
                RoomCount = rooms.Count,
                BedCount = beds.Count,
                FreeBedCount = freeBeds,
                FullyFreeRoomCount = fullyFreeRooms
            });
        }

        // 7. Формуємо загальний звіт
        report.Departments = departmentReports;
        report.TotalRoomCount = departmentReports.Sum(d => d.RoomCount);
        report.TotalBedCount = departmentReports.Sum(d => d.BedCount);

        return report;
    }

    public async Task<LaboratoryReportDto> GetLaboratoryReportAsync(DateTime startDate, DateTime endDate,
        int? hospitalId, int? clinicId)
    {
        var filterDescription = $"Звіт з {startDate.ToShortDateString()} по {endDate.ToShortDateString()}";

        // 1. Починаємо з базового запиту до IQueryable
        var query = (await _labAnalysisRepo.FindByConditionAsync(
            a => a.AnalysisDate.Date >= startDate.Date && a.AnalysisDate.Date <= endDate.Date
        )).AsQueryable();

        // 2. Фільтруємо заклади (якщо треба)
        if (hospitalId.HasValue)
        {
            // Отримуємо ID лабораторій, які обслуговують цю лікарню
            var labIds = (await _laboratoryRepo.FindByConditionAsync(
                l => l.Hospitals.Any(h => h.Id == hospitalId.Value)
            )).Select(l => l.Id);

            query = query.Where(a => labIds.Contains(a.LaboratoryId));
            filterDescription += $", Лікарня ID: {hospitalId.Value}";
        }
        else if (clinicId.HasValue)
        {
            // Отримуємо ID лабораторій, які обслуговують цю клініку
            var labIds = (await _laboratoryRepo.FindByConditionAsync(
                l => l.Clinics.Any(c => c.Id == clinicId.Value)
            )).Select(l => l.Id);

            query = query.Where(a => labIds.Contains(a.LaboratoryId));
            filterDescription += $", Клініка ID: {clinicId.Value}";
        }
        else
        {
            filterDescription += ", Всі медичні установи";
        }

        // 3. Виконуємо запит та рахуємо
        var analyses = query.ToList();

        var totalDays = (int)(endDate.Date - startDate.Date).TotalDays + 1;
        var totalAnalyses = analyses.Count;
        var average = (totalDays > 0) ? (double)totalAnalyses / totalDays : 0;

        // 4. (Опціонально) Групуємо по днях
        var dailyCounts = analyses
            .GroupBy(a => a.AnalysisDate.Date)
            .Select(g => new DailyLabCountDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        // 5. Формуємо DTO
        return new LaboratoryReportDto
        {
            FilterDescription = filterDescription,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            TotalDaysInPeriod = totalDays,
            TotalAnalysesConducted = totalAnalyses,
            AverageAnalysesPerDay = Math.Round(average, 2),
            DailyCounts = dailyCounts
        };
    }

    public async Task<IEnumerable<PatientOperationDto>> GetPatientOperationReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? hospitalId,
        int? clinicId,
        int? doctorId)
    {
        // 1. Отримуємо всі операції зі зв'язками
        var allOperations = await _operationRepo.GetAllWithAssociationsAsync();

        // 2. Фільтруємо в пам'яті
        var filteredOps = allOperations
            .Where(op => op.Date.Date >= startDate.Date && op.Date.Date <= endDate.Date);

        // Фільтр по лікарю
        if (doctorId.HasValue)
        {
            filteredOps = filteredOps.Where(op => op.DoctorId == doctorId.Value);
        }

        // Фільтр по місцю (АБО лікарня, АБО клініка)
        if (hospitalId.HasValue)
        {
            filteredOps = filteredOps.Where(op => op.HospitalId == hospitalId.Value);
        }
        else if (clinicId.HasValue)
        {
            filteredOps = filteredOps.Where(op => op.ClinicId == clinicId.Value);
        }
        // (Якщо hospitalId і clinicId = null, поверне для всіх закладів)

        // 3. Мапимо результат в DTO
        var resultDto = filteredOps.Select(op => new PatientOperationDto
        {
            PatientId = op.PatientId,
            PatientFullName = op.Patient?.FullName ?? "N/A",
            OperationId = op.Id,
            OperationType = op.Type,
            OperationDate = op.Date,
            DoctorName = op.Doctor?.FullName ?? "N/A",

            // Визначаємо назву місця
            LocationName = op.Hospital != null
                ? op.Hospital.Name
                : (op.Clinic?.Name ?? "N/A")
        });

        return resultDto;
    }

    public async Task<IEnumerable<PatientListDto>> GetPatientsByClinicAndSpecialtyAsync(int clinicId, string specialty)
    {
        // 1. Знаходимо ID лікарів потрібного профілю
        // (Використовуємо той самий "хак" з EF.Property, що й у Запиті 1)
        var doctorIdsWithSpecialty = (await _staffRepo
                .FindByConditionAsync(s =>
                    EF.Property<string>(s, "Specialty") == specialty))
            .Select(s => s.Id);

        // 2. Знаходимо M:M зв'язки, які відповідають клініці та профілю
        var assignments = await _clinicAssignmentRepo.FindByConditionAsync(
            a => a.ClinicId == clinicId &&
                 doctorIdsWithSpecialty.Contains(a.DoctorId)
        );

        // 3. Отримуємо унікальні ID пацієнтів з цих зв'язків
        var patientIds = assignments.Select(a => a.PatientId).Distinct();

        // 4. Дістаємо пацієнтів за цими ID
        var patients = await _patientRepo.FindByConditionAsync(p => patientIds.Contains(p.Id));

        // 5. Мапимо в DTO
        return patients.Select(p => new PatientListDto
        {
            Id = p.Id,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            HealthStatus = p.HealthStatus
        });
    }

    public async Task<IEnumerable<PatientListDto>> GetInpatientReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? hospitalId,
        int? doctorId)
    {
        // 1. Отримуємо всі госпіталізації зі зв'язками
        var allAdmissions = await _admissionRepo.GetAllWithAssociationsAsync();

        // 2. Фільтруємо в пам'яті
        var filteredAdmissions = allAdmissions
            // Пацієнт "пройшов лікування" (виписаний) у вказаний період
            .Where(a => a.DischargeDate.HasValue && // Має бути виписаний
                        a.DischargeDate.Value.Date >= startDate.Date &&
                        a.DischargeDate.Value.Date <= endDate.Date);

        // Фільтр по лікарні
        if (hospitalId.HasValue)
        {
            filteredAdmissions = filteredAdmissions.Where(a => a.HospitalId == hospitalId.Value);
        }

        // Фільтр по лікарю
        if (doctorId.HasValue)
        {
            filteredAdmissions = filteredAdmissions.Where(a => a.AttendingDoctorId == doctorId.Value);
        }

        // 3. Мапимо результат в DTO.
        // Нам потрібен список унікальних пацієнтів (один пацієнт міг 2 рази лікуватися)
        var resultDto = filteredAdmissions
            .Select(a => a.Patient) // Беремо тільки пацієнтів
            .DistinctBy(p => p.Id) // Отримуємо унікальних пацієнтів
            .Select(p => new PatientListDto
            {
                Id = p.Id,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth,
                HealthStatus = p.HealthStatus
            });

        return resultDto;
    }

    public async Task<DoctorReportDto> GetDoctorReportAsync(
        int? hospitalId,
        int? clinicId,
        string? specialty,
        int? minWorkExperience,
        AcademicDegree? degree,
        AcademicTitle? title)
    {
        var filterDescription = "Звіт по лікарях. ";

        // 1. Починаємо з IQueryable<Staff>. Нам потрібні *всі* Staff, 
        // бо ми будемо фільтрувати по TPH властивостях (Specialty, Degree, Title)
        var query = (await _staffRepo.GetAllAsync()).AsQueryable();

        // 2. Фільтруємо, щоб залишились тільки лікарі (не SupportStaff)
        // Ми можемо це зробити, перевіривши, що Specialty НЕ null
        // (оскільки це [Required] поле в Doctor, але не існує в SupportStaff)
        query = query.Where(s => EF.Property<string>(s, "Specialty") != null);

        // 3. Застосовуємо фільтри
        if (hospitalId.HasValue)
        {
            query = query.Where(s => s.Employments.Any(e => e.HospitalId == hospitalId.Value));
            filterDescription += $"Лікарня ID: {hospitalId.Value}. ";
        }
        else if (clinicId.HasValue)
        {
            query = query.Where(s => s.Employments.Any(e => e.ClinicId == clinicId.Value));
            filterDescription += $"Клініка ID: {clinicId.Value}. ";
        }
        else
        {
            filterDescription += "Всі установи. ";
        }

        if (!string.IsNullOrEmpty(specialty))
        {
            query = query.Where(s => EF.Property<string>(s, "Specialty") == specialty);
            filterDescription += $"Профіль: {specialty}. ";
        }

        if (minWorkExperience.HasValue)
        {
            query = query.Where(s => s.WorkExperienceYears >= minWorkExperience.Value);
            filterDescription += $"Стаж від: {minWorkExperience.Value} р. ";
        }

        if (degree.HasValue)
        {
            query = query.Where(s => EF.Property<AcademicDegree?>(s, "AcademicDegree") == degree.Value);
            filterDescription += $"Ступінь: {degree}. ";
        }

        if (title.HasValue)
        {
            query = query.Where(s => EF.Property<AcademicTitle?>(s, "AcademicTitle") == title.Value);
            filterDescription += $"Звання: {title}. ";
        }

        // 4. Виконуємо запит
        var doctors = query.ToList();

        // 5. Мапимо в DTO
        var resultDto = new DoctorReportDto
        {
            TotalCount = doctors.Count,
            FilterDescription = filterDescription.Trim(),
            Doctors = doctors.Select(d => new DoctorDetails
            {
                Id = d.Id,
                FullName = d.FullName,
                WorkExperienceYears = d.WorkExperienceYears,
                // Знову використовуємо EF.Property для читання TPH властивостей
                Specialty = EF.Property<string>(d, "Specialty"),
                AcademicDegree = EF.Property<AcademicDegree?>(d, "AcademicDegree"),
                AcademicTitle = EF.Property<AcademicTitle?>(d, "AcademicTitle")
            }).ToList()
        };

        return resultDto;
    }

    public async Task<DoctorReportDto> GetDoctorOperationReportAsync(
        int minOperationCount,
        string? specialty,
        int? hospitalId,
        int? clinicId)
    {
        var filterDescription = $"Звіт по лікарях з >= {minOperationCount} операціями. ";

        // 1. Отримуємо всіх лікарів
        var query = (await _staffRepo.GetAllAsync()).AsQueryable()
            .Where(s => EF.Property<string>(s, "Specialty") != null); // Тільки лікарі

        // 2. Фільтруємо лікарів за місцем роботи
        if (hospitalId.HasValue)
        {
            query = query.Where(s => s.Employments.Any(e => e.HospitalId == hospitalId.Value));
            filterDescription += $"Лікарня ID: {hospitalId.Value}. ";
        }
        else if (clinicId.HasValue)
        {
            query = query.Where(s => s.Employments.Any(e => e.ClinicId == clinicId.Value));
            filterDescription += $"Клініка ID: {clinicId.Value}. ";
        }
        else
        {
            filterDescription += "Всі установи. ";
        }

        // 3. Фільтруємо лікарів за профілем
        if (!string.IsNullOrEmpty(specialty))
        {
            query = query.Where(s => EF.Property<string>(s, "Specialty") == specialty);
            filterDescription += $"Профіль: {specialty}. ";
        }

        // 4. Отримуємо статистику по операціях
        // (Ми не можемо завантажити `op.Doctor.Operations`, бо `op.Doctor` - це `Staff`,
        // тому ми йдемо від `IOperationRepository`)

        // Отримуємо ID всіх лікарів, що пройшли фільтрацію вище
        var doctorIds = query.Select(d => d.Id).ToList();

        // Отримуємо *всі* операції для *цих* лікарів
        var operations = await _operationRepo.FindByConditionAsync(op => doctorIds.Contains(op.DoctorId));

        // Групуємо операції по DoctorId, щоб отримати лічильник
        var operationCounts = operations
            .GroupBy(op => op.DoctorId)
            .ToDictionary(group => group.Key, group => group.Count());

        // 5. Фінальна фільтрація лікарів
        var finalDoctors = query.ToList() // Матеріалізуємо список лікарів
            .Where(d =>
                // Перевіряємо, чи є лікар у статистиці
                operationCounts.ContainsKey(d.Id) &&
                // Перевіряємо, чи кількість операцій >= мінімальній
                operationCounts[d.Id] >= minOperationCount
            ).ToList();

        // 6. Мапимо в DTO
        var resultDto = new DoctorReportDto
        {
            TotalCount = finalDoctors.Count,
            FilterDescription = filterDescription.Trim(),
            Doctors = finalDoctors.Select(d => new DoctorDetails
            {
                Id = d.Id,
                FullName = d.FullName,
                WorkExperienceYears = d.WorkExperienceYears,
                Specialty = EF.Property<string>(d, "Specialty"),
                AcademicDegree = EF.Property<AcademicDegree?>(d, "AcademicDegree"),
                AcademicTitle = EF.Property<AcademicTitle?>(d, "AcademicTitle")
            }).ToList()
        };

        return resultDto;
    }

    public async Task<SupportStaffReportDto> GetSupportStaffReportAsync(
        SupportRole role,
        int? hospitalId,
        int? clinicId)
    {
        var filterDescription = $"Звіт по обслуговуючому персоналу. Роль: {role}. ";

        // 1. Починаємо з IQueryable<Staff>
        var query = (await _staffRepo.GetAllAsync()).AsQueryable();

        // 2. Фільтруємо, щоб залишились *тільки* SupportStaff
        // OfType<T>() автоматично додасть потрібний 'WHERE [StaffType] = "SupportStaff"'
        var supportStaffQuery = query.OfType<SupportStaff>();

        // 3. Фільтруємо по ролі (спеціальності)
        supportStaffQuery = supportStaffQuery.Where(s => s.Role == role);

        // 4. Фільтруємо по місцю роботи
        if (hospitalId.HasValue)
        {
            supportStaffQuery = supportStaffQuery.Where(s => s.Employments.Any(e => e.HospitalId == hospitalId.Value));
            filterDescription += $"Лікарня ID: {hospitalId.Value}.";
        }
        else if (clinicId.HasValue)
        {
            supportStaffQuery = supportStaffQuery.Where(s => s.Employments.Any(e => e.ClinicId == clinicId.Value));
            filterDescription += $"Kлініка ID: {clinicId.Value}.";
        }
        else
        {
            filterDescription += "Всі установи.";
        }

        // 5. Виконуємо запит
        var staffList = supportStaffQuery.ToList();

        // 6. Мапимо в DTO
        var resultDto = new SupportStaffReportDto
        {
            TotalCount = staffList.Count,
            FilterDescription = filterDescription.Trim(),
            Staff = staffList.Select(s => new SupportStaffDetails
            {
                Id = s.Id,
                FullName = s.FullName,
                Role = s.Role,
                WorkExperienceYears = s.WorkExperienceYears
            }).ToList()
        };

        return resultDto;
    }

    public async Task<IEnumerable<DoctorPerformanceDto>> GetDoctorPerformanceReportAsync(int? hospitalId, int? clinicId)
    {
        // 1. Отримуємо всі операції зі зв'язками
        var operationsQuery = (await _operationRepo.GetAllWithAssociationsAsync()).AsQueryable();

        // 2. Фільтруємо операції за місцем
        if (hospitalId.HasValue)
        {
            operationsQuery = operationsQuery.Where(op => op.HospitalId == hospitalId.Value);
        }
        else if (clinicId.HasValue)
        {
            operationsQuery = operationsQuery.Where(op => op.ClinicId == clinicId.Value);
        }

        var operations = operationsQuery.ToList();

        // 3. Фільтруємо лікарів, які ВЗАГАЛІ можуть проводити операції
        // (Це Surgeon, Dentist, Gynecologist згідно твоїх моделей)
        var operatingDoctorIds = (await _staffRepo.GetAllAsync())
            .OfType<Doctor>() // Беремо тільки лікарів
            .Where(d => d is Surgeon || d is Dentist || d is Gynecologist)
            .Select(d => d.Id);

        // 4. Групуємо відфільтровані операції по лікарях
        var report = operations
            .Where(op => operatingDoctorIds.Contains(op.DoctorId)) // Беремо операції тільки цих лікарів
            .GroupBy(op => op.Doctor) // Групуємо по об'єкту Doctor
            .Select(g =>
            {
                int total = g.Count();
                int fatal = g.Count(op => op.IsFatal);
                double rate = (total > 0) ? ((double)fatal / total) * 100 : 0;

                return new DoctorPerformanceDto
                {
                    DoctorId = g.Key.Id,
                    DoctorFullName = g.Key.FullName,
                    Specialty = EF.Property<string>(g.Key, "Specialty"),
                    TotalOperations = total,
                    FatalOperations = fatal,
                    FatalityRatePercent = Math.Round(rate, 2)
                };
            })
            .OrderByDescending(dto => dto.FatalityRatePercent) // Сортуємо
            .ToList();

        return report;
    }

    public async Task<DepartmentOccupancyReportDto> GetDepartmentOccupancyReportAsync(int departmentId)
    {
        // 1. Знаходимо відділення
        var department = await _departmentRepo.GetByIdAsync(departmentId);
        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        // 2. Знаходимо всі палати в цьому відділенні
        var rooms = (await _roomRepo.FindByConditionAsync(r => r.DepartmentId == departmentId)).ToList();
        var roomIds = rooms.Select(r => r.Id);

        // 3. Знаходимо всі ліжка в цих палатах
        var beds = (await _bedRepo.FindByConditionAsync(b => roomIds.Contains(b.RoomId))).ToList();

        var roomReports = new List<RoomOccupancyDto>();

        // 4. Розраховуємо статистику для кожної палати
        foreach (var room in rooms)
        {
            int roomCapacity = room.Capacity; //
            int occupiedBeds = beds.Count(b => b.RoomId == room.Id && b.IsOccupied); //
            double percent = (roomCapacity > 0)
                ? (double)occupiedBeds / roomCapacity * 100
                : 0;

            roomReports.Add(new RoomOccupancyDto
            {
                RoomId = room.Id,
                RoomNumber = room.Number,
                RoomCapacity = roomCapacity,
                OccupiedBeds = occupiedBeds,
                OccupancyPercent = Math.Round(percent, 2)
            });
        }

        // 5. Розраховуємо загальну статистику по відділенню
        int totalCapacity = roomReports.Sum(r => r.RoomCapacity);
        int totalOccupied = roomReports.Sum(r => r.OccupiedBeds);
        double overallPercent = (totalCapacity > 0)
            ? (double)totalOccupied / totalCapacity * 100
            : 0;

        // 6. Формуємо DTO
        return new DepartmentOccupancyReportDto
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            TotalRooms = rooms.Count,
            TotalCapacity = totalCapacity,
            TotalOccupiedBeds = totalOccupied,
            OverallOccupancyPercent = Math.Round(overallPercent, 2),
            Rooms = roomReports.OrderBy(r => r.RoomNumber).ToList()
        };
    }
}