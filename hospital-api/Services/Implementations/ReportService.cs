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
    private readonly IStaffRepository _staffRepo;

    private readonly IBuildingRepository _buildingRepo;

    private readonly IHospitalRepository _hospitalRepo;
    private readonly IDepartmentRepository _departmentRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IBedRepository _bedRepo;

    private readonly ILabAnalysisRepository _labAnalysisRepo;
    private readonly ILaboratoryRepository _laboratoryRepo;

    private readonly IOperationRepository _operationRepo;

    private readonly IClinicDoctorAssignmentRepository _clinicAssignmentRepo;
    private readonly IPatientRepository _patientRepo;

    private readonly IAdmissionRepository _admissionRepo;

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
        IAdmissionRepository admissionRepo)
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
        _admissionRepo = admissionRepo;
    }

    public async Task<AppointmentCountDto> GetDailyAppointmentCountAsync(DateTime date, int? doctorId, int? clinicId,
        string? specialty)
    {
        if (date.Kind == DateTimeKind.Unspecified)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        // Використовуємо GetAll() + Include(), щоб завантажити лікаря
        var query = _appointmentRepo.GetAll()
            .Include(a => a.Doctor)
            .Where(a => a.VisitDateTime.Date == date.Date);

        var filterDescription = $"Звіт за {date.ToShortDateString()}";

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
                // ✅ ВИПРАВЛЕНО: Використовуємо OfType<Doctor>() замість EF.Property
                var doctorIdsInClinicWithSpecialty = await _staffRepo.GetAll()
                    .OfType<Doctor>()
                    .Where(s => s.Employments.Any(e => e.ClinicId == clinicId.Value) && s.Specialty == specialty)
                    .Select(s => s.Id)
                    .ToListAsync();

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
            // ✅ ВИПРАВЛЕНО: Використовуємо OfType<Doctor>()
            var doctorIdsWithSpecialty = await _staffRepo.GetAll()
                .OfType<Doctor>()
                .Where(s => s.Specialty == specialty)
                .Select(s => s.Id)
                .ToListAsync();

            query = query.Where(a => doctorIdsWithSpecialty.Contains(a.DoctorId));
            filterDescription += $", Профіль (всі заклади): {specialty}";
        }
        else
        {
            filterDescription += ", Всі лікарі, всі заклади";
        }

        var appointments = await query.ToListAsync();

        var countByDoctor = appointments
            .GroupBy(a => a.DoctorId)
            .Select(g => new DoctorAppointmentCountDto
            {
                DoctorId = g.Key,
                DoctorFullName = g.First().Doctor?.FullName ?? $"DoctorID {g.Key}",
                PatientCount = g.Count()
            })
            .ToList();

        return new AppointmentCountDto
        {
            FilterDescription = filterDescription,
            ReportDate = date.Date,
            PatientCount = appointments.Count,
            CountByDoctor = countByDoctor
        };
    }

    public async Task<HospitalCapacityReportDto> GetHospitalCapacityReportAsync(int hospitalId)
    {
        // Код без змін
        var hospital = await _hospitalRepo.GetByIdAsync(hospitalId);
        if (hospital == null)
            throw new KeyNotFoundException("Hospital not found.");

        var report = new HospitalCapacityReportDto
        {
            HospitalId = hospital.Id,
            HospitalName = hospital.Name
        };

        var departmentReports = new List<DepartmentCapacityDto>();

        var buildings = await _buildingRepo.FindByConditionAsync(b => b.HospitalId == hospitalId);
        var buildingIds = buildings.Select(b => b.Id);

        var departments = await _departmentRepo.FindByConditionAsync(d => buildingIds.Contains(d.BuildingId));

        foreach (var dept in departments)
        {
            var rooms = (await _roomRepo.FindByConditionAsync(r => r.DepartmentId == dept.Id)).ToList();
            var roomIds = rooms.Select(r => r.Id).ToList();

            var beds = (await _bedRepo.FindByConditionAsync(b => roomIds.Contains(b.RoomId))).ToList();

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

        report.Departments = departmentReports;
        report.TotalRoomCount = departmentReports.Sum(d => d.RoomCount);
        report.TotalBedCount = departmentReports.Sum(d => d.BedCount);

        return report;
    }

    public async Task<LaboratoryReportDto> GetLaboratoryReportAsync(DateTime startDate, DateTime endDate,
        int? hospitalId, int? clinicId)
    {
        if (startDate.Kind == DateTimeKind.Unspecified) startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        if (endDate.Kind == DateTimeKind.Unspecified) endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var filterDescription = $"Звіт з {startDate.ToShortDateString()} по {endDate.ToShortDateString()}";

        var query = _labAnalysisRepo.GetAll()
            .Where(a => a.AnalysisDate.Date >= startDate.Date && a.AnalysisDate.Date <= endDate.Date);

        if (hospitalId.HasValue)
        {
            var labIds = (await _laboratoryRepo.FindByConditionAsync(
                l => l.Hospitals.Any(h => h.Id == hospitalId.Value)
            )).Select(l => l.Id);

            query = query.Where(a => labIds.Contains(a.LaboratoryId));
            filterDescription += $", Лікарня ID: {hospitalId.Value}";
        }
        else if (clinicId.HasValue)
        {
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

        var analyses = await query.ToListAsync();

        var totalDays = (int)(endDate.Date - startDate.Date).TotalDays + 1;
        var totalAnalyses = analyses.Count;
        var average = (totalDays > 0) ? (double)totalAnalyses / totalDays : 0;

        var dailyCounts = analyses
            .GroupBy(a => a.AnalysisDate.Date)
            .Select(g => new DailyLabCountDto
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

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
        if (startDate.Kind == DateTimeKind.Unspecified) startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        if (endDate.Kind == DateTimeKind.Unspecified) endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        // GetAllWithAssociationsAsync вже включає Doctor, Clinic, Hospital
        var allOperations = await _operationRepo.GetAllWithAssociationsAsync();

        var filteredOps = allOperations
            .Where(op => op.Date.Date >= startDate.Date && op.Date.Date <= endDate.Date);

        if (doctorId.HasValue)
        {
            filteredOps = filteredOps.Where(op => op.DoctorId == doctorId.Value);
        }

        if (hospitalId.HasValue)
        {
            filteredOps = filteredOps.Where(op => op.HospitalId == hospitalId.Value);
        }
        else if (clinicId.HasValue)
        {
            filteredOps = filteredOps.Where(op => op.ClinicId == clinicId.Value);
        }

        var resultDto = filteredOps.Select(op => new PatientOperationDto
        {
            PatientId = op.PatientId,
            PatientFullName = op.Patient?.FullName ?? "N/A",
            OperationId = op.Id,
            OperationType = op.Type,
            OperationDate = op.Date,
            DoctorName = op.Doctor?.FullName ?? "N/A",
            LocationName = op.Hospital != null
                ? op.Hospital.Name
                : (op.Clinic?.Name ?? "N/A")
        });

        return resultDto;
    }

    public async Task<IEnumerable<PatientListDto>> GetPatientsByClinicAndSpecialtyAsync(int clinicId, string specialty)
    {
        // ✅ ВИПРАВЛЕНО: Використовуємо OfType<Doctor>() для безпечного доступу до Specialty
        var doctorIdsWithSpecialty = await _staffRepo.GetAll()
            .OfType<Doctor>()
            .Where(d => d.Specialty == specialty)
            .Select(s => s.Id)
            .ToListAsync();

        // Решта логіки залишається незмінною
        var assignments = await _clinicAssignmentRepo.FindByConditionAsync(
            a => a.ClinicId == clinicId &&
                 doctorIdsWithSpecialty.Contains(a.DoctorId)
        );

        var patientIds = assignments.Select(a => a.PatientId).Distinct();

        var patients = await _patientRepo.FindByConditionAsync(p => patientIds.Contains(p.Id));

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
        if (startDate.Kind == DateTimeKind.Unspecified) startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        if (endDate.Kind == DateTimeKind.Unspecified) endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        var allAdmissions = await _admissionRepo.GetAllWithAssociationsAsync();

        var filteredAdmissions = allAdmissions
            .Where(a => a.DischargeDate.HasValue &&
                        a.DischargeDate.Value.Date >= startDate.Date &&
                        a.DischargeDate.Value.Date <= endDate.Date);

        if (hospitalId.HasValue)
        {
            filteredAdmissions = filteredAdmissions.Where(a => a.HospitalId == hospitalId.Value);
        }

        if (doctorId.HasValue)
        {
            filteredAdmissions = filteredAdmissions.Where(a => a.AttendingDoctorId == doctorId.Value);
        }

        var resultDto = filteredAdmissions
            .Select(a => a.Patient)
            .DistinctBy(p => p.Id)
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

        // ✅ ВИПРАВЛЕНО: Починаємо одразу з OfType<Doctor>(), щоб працювати з лікарями
        var query = _staffRepo.GetAll().OfType<Doctor>();

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
            query = query.Where(s => s.Specialty == specialty);
            filterDescription += $"Профіль: {specialty}. ";
        }

        if (minWorkExperience.HasValue)
        {
            query = query.Where(s => s.WorkExperienceYears >= minWorkExperience.Value);
            filterDescription += $"Стаж від: {minWorkExperience.Value} р. ";
        }

        if (degree.HasValue)
        {
            query = query.Where(s => s.AcademicDegree == degree.Value);
            filterDescription += $"Ступінь: {degree}. ";
        }

        if (title.HasValue)
        {
            query = query.Where(s => s.AcademicTitle == title.Value);
            filterDescription += $"Звання: {title}. ";
        }

        var doctors = await query.ToListAsync();

        var resultDto = new DoctorReportDto
        {
            TotalCount = doctors.Count,
            FilterDescription = filterDescription.Trim(),
            Doctors = doctors.Select(d => new DoctorDetails
            {
                Id = d.Id,
                FullName = d.FullName,
                WorkExperienceYears = d.WorkExperienceYears,
                Specialty = d.Specialty,
                AcademicDegree = d.AcademicDegree,
                AcademicTitle = d.AcademicTitle
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

        // ✅ ВИПРАВЛЕНО: Використовуємо OfType<Doctor>()
        var query = _staffRepo.GetAll().OfType<Doctor>();

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
            query = query.Where(s => s.Specialty == specialty);
            filterDescription += $"Профіль: {specialty}. ";
        }

        // Завантажуємо ID лікарів, що підходять
        var doctorIds = await query.Select(d => d.Id).ToListAsync();

        // Отримуємо операції для цих лікарів
        var operations = await _operationRepo.FindByConditionAsync(op => doctorIds.Contains(op.DoctorId));

        // Рахуємо
        var operationCounts = operations
            .GroupBy(op => op.DoctorId)
            .ToDictionary(group => group.Key, group => group.Count());

        // Фільтруємо список лікарів (завантажуємо повні дані тих, хто пройшов по ліміту)
        var finalDoctors = await query
            .Where(d => doctorIds.Contains(d.Id)) // (Фактично redundant, але безпечно)
            .ToListAsync();
            
        finalDoctors = finalDoctors
            .Where(d => operationCounts.ContainsKey(d.Id) && operationCounts[d.Id] >= minOperationCount)
            .ToList();

        var resultDto = new DoctorReportDto
        {
            TotalCount = finalDoctors.Count,
            FilterDescription = filterDescription.Trim(),
            Doctors = finalDoctors.Select(d => new DoctorDetails
            {
                Id = d.Id,
                FullName = d.FullName,
                WorkExperienceYears = d.WorkExperienceYears,
                Specialty = d.Specialty,
                AcademicDegree = d.AcademicDegree,
                AcademicTitle = d.AcademicTitle
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

        // ✅ ВИПРАВЛЕНО: Використовуємо OfType<SupportStaff>()
        var supportStaffQuery = _staffRepo.GetAll().OfType<SupportStaff>();

        supportStaffQuery = supportStaffQuery.Where(s => s.Role == role);

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

        var staffList = await supportStaffQuery.ToListAsync();

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
        var operationsQuery = (await _operationRepo.GetAllWithAssociationsAsync()).AsQueryable();

        if (hospitalId.HasValue)
        {
            operationsQuery = operationsQuery.Where(op => op.HospitalId == hospitalId.Value);
        }
        else if (clinicId.HasValue)
        {
            operationsQuery = operationsQuery.Where(op => op.ClinicId == clinicId.Value);
        }

        var operations = operationsQuery.ToList();

        // ✅ ВИПРАВЛЕНО: Використовуємо OfType<Doctor>() і фільтр по спеціальності
        var operatingDoctorIds = await _staffRepo.GetAll()
            .OfType<Doctor>()
            .Where(d => d.Specialty == "Surgeon" || d.Specialty == "Dentist" || d.Specialty == "Gynecologist")
            .Select(d => d.Id)
            .ToListAsync();

        var report = operations
            .Where(op => operatingDoctorIds.Contains(op.DoctorId))
            .GroupBy(op => op.Doctor)
            .Select(g =>
            {
                int total = g.Count();
                int fatal = g.Count(op => op.IsFatal);
                double rate = (total > 0) ? ((double)fatal / total) * 100 : 0;

                // Оскільки g.Key - це Staff (через навігаційну властивість Operation.Doctor),
                // нам треба переконатися, що ми можемо взяти Specialty.
                // Але ми вже відфільтрували за operatingDoctorIds, тому це точно Doctor.
                // Найбезпечніше - взяти з БД або зробити Cast, якщо об'єкт завантажений як Doctor.
                // Тут для спрощення візьмемо Specialty через dynamic або припущення,
                // але оскільки g.Key завантажений через EF Include, він може бути проксі Staff.
                // Краще знайти лікаря з кешу або явно.
                // У цьому контексті g.Key.GetType().Name поверне тип (Surgeon і т.д.).

                // Спробуємо отримати спеціальність через рефлексію або хак, оскільки Staff не має її.
                // Але найкраще - просто відобразити ім'я, а спеціальність ми вже знаємо з фільтра.
                // Або використати операцію Cast, якщо об'єкт в пам'яті.
                var doctor = g.Key as Doctor;

                return new DoctorPerformanceDto
                {
                    DoctorId = g.Key.Id,
                    DoctorFullName = g.Key.FullName,
                    Specialty = doctor?.Specialty ?? "N/A",
                    TotalOperations = total,
                    FatalOperations = fatal,
                    FatalityRatePercent = Math.Round(rate, 2)
                };
            })
            .OrderByDescending(dto => dto.FatalityRatePercent)
            .ToList();

        return report;
    }

    public async Task<DepartmentOccupancyReportDto> GetDepartmentOccupancyReportAsync(int departmentId)
    {
        // Без змін
        var department = await _departmentRepo.GetByIdAsync(departmentId);
        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        var rooms = (await _roomRepo.FindByConditionAsync(r => r.DepartmentId == departmentId)).ToList();
        var roomIds = rooms.Select(r => r.Id);

        var beds = (await _bedRepo.FindByConditionAsync(b => roomIds.Contains(b.RoomId))).ToList();

        var roomReports = new List<RoomOccupancyDto>();

        foreach (var room in rooms)
        {
            int roomCapacity = room.Capacity;
            int occupiedBeds = beds.Count(b => b.RoomId == room.Id && b.IsOccupied);
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

        int totalCapacity = roomReports.Sum(r => r.RoomCapacity);
        int totalOccupied = roomReports.Sum(r => r.OccupiedBeds);
        double overallPercent = (totalCapacity > 0)
            ? (double)totalOccupied / totalCapacity * 100
            : 0;

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