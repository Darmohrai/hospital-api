using System.Text;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class SupportStaffService : ISupportStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public SupportStaffService(
        IStaffRepository staffRepository,
        IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    // --- Реалізація методів ---

    public async Task<IEnumerable<SupportStaff>> GetAllAsync()
    {
        // Використовуємо загальний репозиторій і фільтруємо за типом
        return await _staffRepository.GetAll().OfType<SupportStaff>().ToListAsync();
    }

    public async Task<SupportStaff?> GetByIdAsync(int id)
    {
        // Отримуємо Staff і перевіряємо, чи він є типу SupportStaff
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as SupportStaff;
    }

    // ОНОВЛЕНО: Додані параметри hospitalId та clinicId
    public async Task CreateAsync(SupportStaff staff, int? hospitalId, int? clinicId)
    {
        if (string.IsNullOrWhiteSpace(staff.FullName))
        {
            throw new ArgumentException("Support staff full name is required.");
        }

        // 1. Спочатку створюємо самого співробітника, щоб отримати його ID
        await _staffRepository.AddAsync(staff);

        // 2. Якщо вказано місце роботи, створюємо запис в Employment
        if (hospitalId.HasValue || clinicId.HasValue)
        {
            var employment = new Employment
            {
                StaffId = staff.Id,
                HospitalId = hospitalId,
                ClinicId = clinicId
            };
            await _employmentRepository.AddAsync(employment);
        }
    }

    // ОНОВЛЕНО: Додані параметри hospitalId та clinicId
    public async Task UpdateAsync(SupportStaff staffFromRequest, int? hospitalId, int? clinicId)
    {
        // 1. Оновлюємо основні дані співробітника (ім'я, роль, досвід)
        await _staffRepository.UpdateAsync(staffFromRequest);

        // 2. Отримуємо поточне працевлаштування співробітника
        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(staffFromRequest.Id);
        var currentEmployment = employments.FirstOrDefault();

        // 3. Логіка оновлення місця роботи
        if (currentEmployment != null)
        {
            // Якщо запис вже є - оновлюємо його
            currentEmployment.HospitalId = hospitalId;
            currentEmployment.ClinicId = clinicId;
            await _employmentRepository.UpdateAsync(currentEmployment);
        }
        else if (hospitalId.HasValue || clinicId.HasValue)
        {
            // Якщо запису не було, але ми обрали лікарню/поліклініку - створюємо новий
            var newEmployment = new Employment
            {
                StaffId = staffFromRequest.Id,
                HospitalId = hospitalId,
                ClinicId = clinicId
            };
            await _employmentRepository.AddAsync(newEmployment);
        }
    }

    public async Task DeleteAsync(int id)
    {
        // Видаляємо через загальний репозиторій
        // (Cascading delete в БД має подбати про видалення Employment, або це зробить EF Core)
        await _staffRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role)
    {
        return await _staffRepository.GetSupportStaffByRoleAsync(role);
    }

    public async Task<IEnumerable<SupportStaff>> GetByClinicAsync(int clinicId, SupportRole? role = null)
    {
        return await _staffRepository.GetSupportStaffByClinicAsync(clinicId, role);
    }

    public async Task<IEnumerable<SupportStaff>> GetByHospitalAsync(int hospitalId, SupportRole? role = null)
    {
        return await _staffRepository.GetSupportStaffByHospitalAsync(hospitalId, role);
    }

    public async Task<string> GetProfileSummaryAsync(int staffId)
    {
        var staff = await GetByIdAsync(staffId);
        if (staff == null)
        {
            return "Support staff not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль співробітника: {staff.FullName}");
        summaryBuilder.AppendLine($"Роль: {staff.Role}");
        summaryBuilder.AppendLine($"Досвід: {staff.WorkExperienceYears} років");

        // Отримуємо місце роботи
        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(staffId);
        summaryBuilder.AppendLine("Місце роботи:");

        var employmentList = employments.ToList();
        if (!employmentList.Any())
        {
            summaryBuilder.AppendLine("- Наразі не працевлаштований (або дані не вказані).");
        }
        else
        {
            foreach (var employment in employmentList)
            {
                // Перевіряємо, де саме працює
                if (employment.HospitalId.HasValue)
                {
                    // Якщо навігаційна властивість Hospital не завантажена, показуємо ID (або треба Include в репозиторії)
                    var hospitalName = employment.Hospital?.Name ?? $"ID: {employment.HospitalId}";
                    summaryBuilder.AppendLine($"- Лікарня: {hospitalName}");
                }
                
                if (employment.ClinicId.HasValue)
                {
                    var clinicName = employment.Clinic?.Name ?? $"ID: {employment.ClinicId}";
                    summaryBuilder.AppendLine($"- Поліклініка: {clinicName}");
                }

                // Якщо записано, але обидва null (теоретично можливо при ручному редагуванні БД)
                if (!employment.HospitalId.HasValue && !employment.ClinicId.HasValue)
                {
                    summaryBuilder.AppendLine("- Закріплений запис про працевлаштування без прив'язки до закладу.");
                }
            }
        }

        return summaryBuilder.ToString();
    }
}