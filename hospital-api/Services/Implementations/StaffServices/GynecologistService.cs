using System.Text;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class GynecologistService : IGynecologistService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public GynecologistService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Gynecologist>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Gynecologist>().ToListAsync();
    }

    public async Task<Gynecologist?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Gynecologist;
    }

    public async Task<ServiceResponse<Gynecologist>> CreateAsync(Gynecologist gynecologist)
    {
        if (string.IsNullOrWhiteSpace(gynecologist.FullName))
        {
            return ServiceResponse<Gynecologist>.Fail("Gynecologist's full name is required.");
        }

        gynecologist.Specialty = "Gynecologist";
        await _staffRepository.AddAsync(gynecologist);
        return ServiceResponse<Gynecologist>.Success(gynecologist);
    }

    public async Task<ServiceResponse<Gynecologist>> UpdateAsync(Gynecologist gynecologist)
    {
        var existing = await GetByIdAsync(gynecologist.Id);
        if (existing == null)
        {
            return ServiceResponse<Gynecologist>.Fail("Gynecologist not found.");
        }

        await _staffRepository.UpdateAsync(gynecologist);
        return ServiceResponse<Gynecologist>.Success(gynecologist);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResponse<bool>.Fail("Gynecologist not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Gynecologist>> GetByMinimumOperationCountAsync(int minOperations)
    {
        return await _staffRepository.GetAll()
            .OfType<Gynecologist>()
            .Where(g => g.OperationCount >= minOperations)
            .ToListAsync();
    }

    public async Task<string> GetProfileSummaryAsync(int gynecologistId)
    {
        var gynecologist = await GetByIdAsync(gynecologistId);
        if (gynecologist == null)
        {
            return "Gynecologist not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль лікаря: {gynecologist.FullName}");
        summaryBuilder.AppendLine($"Спеціалізація: {gynecologist.Specialty}");
        summaryBuilder.AppendLine($"Кількість операцій: {gynecologist.OperationCount}");
        summaryBuilder.AppendLine($"Кількість летальних операцій: {gynecologist.FatalOperationCount}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(gynecologistId);
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