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

    public async Task CreateAsync(SupportStaff staff)
    {
        if (string.IsNullOrWhiteSpace(staff.FullName))
        {
            throw new ArgumentException("Support staff full name is required.");
        }

        // Додаємо через загальний репозиторій
        await _staffRepository.AddAsync(staff);
    }

    public async Task UpdateAsync(SupportStaff staff)
    {
        var existingStaff = await GetByIdAsync(staff.Id);
        if (existingStaff == null)
        {
            throw new InvalidOperationException("Support staff not found.");
        }

        // Оновлюємо через загальний репозиторій
        await _staffRepository.UpdateAsync(staff);
    }

    public async Task DeleteAsync(int id)
    {
        // Видаляємо через загальний репозиторій
        await _staffRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role)
    {
        // Викликаємо відповідний метод з IStaffRepository
        return await _staffRepository.GetSupportStaffByRoleAsync(role);
    }

    public async Task<IEnumerable<SupportStaff>> GetByClinicAsync(int clinicId, SupportRole? role = null)
    {
        // Викликаємо відповідний метод з IStaffRepository
        return await _staffRepository.GetSupportStaffByClinicAsync(clinicId, role);
    }

    public async Task<IEnumerable<SupportStaff>> GetByHospitalAsync(int hospitalId, SupportRole? role = null)
    {
        // Викликаємо відповідний метод з IStaffRepository
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

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(staffId);
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