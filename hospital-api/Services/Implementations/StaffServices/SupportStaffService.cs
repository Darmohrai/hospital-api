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
    
    public async Task<IEnumerable<SupportStaff>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<SupportStaff>().ToListAsync();
    }

    public async Task<SupportStaff?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as SupportStaff;
    }

    public async Task CreateAsync(SupportStaff staff, int? hospitalId, int? clinicId)
    {
        if (string.IsNullOrWhiteSpace(staff.FullName))
        {
            throw new ArgumentException("Support staff full name is required.");
        }

        await _staffRepository.AddAsync(staff);

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

    public async Task UpdateAsync(SupportStaff staffFromRequest, int? hospitalId, int? clinicId)
    {
        await _staffRepository.UpdateAsync(staffFromRequest);

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(staffFromRequest.Id);
        var currentEmployment = employments.FirstOrDefault();

        if (currentEmployment != null)
        {
            currentEmployment.HospitalId = hospitalId;
            currentEmployment.ClinicId = clinicId;
            await _employmentRepository.UpdateAsync(currentEmployment);
        }
        else if (hospitalId.HasValue || clinicId.HasValue)
        {
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
                if (employment.HospitalId.HasValue)
                {
                    var hospitalName = employment.Hospital?.Name ?? $"ID: {employment.HospitalId}";
                    summaryBuilder.AppendLine($"- Лікарня: {hospitalName}");
                }
                
                if (employment.ClinicId.HasValue)
                {
                    var clinicName = employment.Clinic?.Name ?? $"ID: {employment.ClinicId}";
                    summaryBuilder.AppendLine($"- Поліклініка: {clinicName}");
                }

                if (!employment.HospitalId.HasValue && !employment.ClinicId.HasValue)
                {
                    summaryBuilder.AppendLine("- Закріплений запис про працевлаштування без прив'язки до закладу.");
                }
            }
        }

        return summaryBuilder.ToString();
    }
}