using System.Text;
using hospital_api.DTOs.Staff;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Models.StaffAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class NeurologistService : INeurologistService
{
    private readonly IStaffRepository _staffRepository; // Єдиний репозиторій для всього персоналу
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IEmploymentRepository _employmentRepository;

    // ✅ Правильний конструктор з коректними залежностями
    public NeurologistService(
        IStaffRepository staffRepository,
        IEmploymentRepository employmentRepository,
        IHospitalRepository hospitalRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
        _hospitalRepository = hospitalRepository;
    }

    // ✅ Отримання всіх неврологів через загальний репозиторій з фільтрацією по типу
    public async Task<IEnumerable<Neurologist>> GetAllNeurologistsAsync()
    {
        return await _staffRepository.GetAll()
            .OfType<Neurologist>() // Магія TPH: фільтруємо тільки неврологів
            .ToListAsync();
    }

    public async Task<Neurologist?> GetNeurologistByIdAsync(int id)
    {
        return await _staffRepository.GetAll()
            .OfType<Neurologist>()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    // ✅ Повністю переписана логіка працевлаштування
    public async Task<ServiceResponse<Neurologist>> AddNeurologistToHospitalAsync(int hospitalId,
        CreateNeurologistDto dto)
    {
        var hospital = await _hospitalRepository.GetByIdAsync(hospitalId);
        if (hospital == null)
            return ServiceResponse<Neurologist>.Fail($"Hospital with ID {hospitalId} not found.");

        if (!hospital.Specializations.Contains(HospitalSpecialization.Neurologist))
            return ServiceResponse<Neurologist>.Fail(
                "Cannot add a neurologist to a hospital without a neurology specialization.");

        // 1. Створюємо об'єкт Neurologist
        var neurologist = new Neurologist
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            Specialty = "Neurologist", // Спеціальність визначена типом класу
            AcademicDegree = dto.AcademicDegree,
            AcademicTitle = dto.AcademicTitle,
            ExtendedVacationDays = dto.ExtendedVacationDays,
        };

        // 2. Зберігаємо його як звичайного співробітника
        await _staffRepository.AddAsync(neurologist);

        // 3. Створюємо запис про працевлаштування
        var employment = new Employment
        {
            StaffId = neurologist.Id,
            HospitalId = hospitalId
        };
        await _employmentRepository.AddAsync(employment);

        return ServiceResponse<Neurologist>.Success(neurologist);
    }

    public async Task UpdateNeurologistAsync(Neurologist neurologist)
    {
        // Для оновлення ми також працюємо через загальний репозиторій
        await _staffRepository.UpdateAsync(neurologist);
    }

    public async Task DeleteNeurologistAsync(int id)
    {
        await _staffRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Neurologist>> GetNeurologistsByExtendedVacationDaysAsync(int minDays)
    {
        return await _staffRepository.GetAll()
            .OfType<Neurologist>()
            .Where(n => n.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }

    // ✅ Повністю переписаний метод для генерації профілю
    public async Task<string> GetNeurologistProfileSummaryAsync(int neurologistId)
    {
        var neurologist = await GetNeurologistByIdAsync(neurologistId);
        if (neurologist == null)
            return "Neurologist not found.";

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль лікаря: {neurologist.FullName}");
        summaryBuilder.AppendLine($"Спеціалізація: {neurologist.Specialty}");
        summaryBuilder.AppendLine($"Стаж роботи: {neurologist.WorkExperienceYears} років");
        summaryBuilder.AppendLine($"Науковий ступінь: {neurologist.AcademicDegree}");
        summaryBuilder.AppendLine($"Вчене звання: {neurologist.AcademicTitle}");
        summaryBuilder.AppendLine($"Дні додаткової відпустки: {neurologist.ExtendedVacationDays}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(neurologistId);
        summaryBuilder.AppendLine("Місця роботи:");

        var employmentList = employments.ToList();
        if (!employmentList.Any())
        {
            summaryBuilder.AppendLine("- Наразі не працевлаштований.");
        }
        else
        {
            foreach (var employment in employmentList)
            {
                if (employment.Hospital != null)
                    summaryBuilder.AppendLine($"- Лікарня: {employment.Hospital.Name}");
                if (employment.Clinic != null)
                    summaryBuilder.AppendLine($"- Поліклініка: {employment.Clinic.Name}");
            }
        }

        return summaryBuilder.ToString();
    }
}