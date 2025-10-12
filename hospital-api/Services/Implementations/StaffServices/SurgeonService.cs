using System.Text;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class SurgeonService : ISurgeonService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public SurgeonService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Surgeon>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Surgeon>().ToListAsync();
    }

    public async Task<Surgeon?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Surgeon;
    }

    public async Task<ServiceResponse<Surgeon>> CreateAsync(Surgeon surgeon)
    {
        if (string.IsNullOrWhiteSpace(surgeon.FullName))
        {
            return ServiceResponse<Surgeon>.Fail("Surgeon's full name is required.");
        }

        surgeon.Specialty = "Surgeon";
        await _staffRepository.AddAsync(surgeon);
        return ServiceResponse<Surgeon>.Success(surgeon);
    }

    public async Task<ServiceResponse<Surgeon>> UpdateAsync(Surgeon surgeon)
    {
        var existing = await GetByIdAsync(surgeon.Id);
        if (existing == null)
        {
            return ServiceResponse<Surgeon>.Fail("Surgeon not found.");
        }

        await _staffRepository.UpdateAsync(surgeon);
        return ServiceResponse<Surgeon>.Success(surgeon);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResponse<bool>.Fail("Surgeon not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Surgeon>> GetByMinimumOperationCountAsync(int minOperationCount)
    {
        return await _staffRepository.GetAll()
            .OfType<Surgeon>()
            .Where(s => s.OperationCount >= minOperationCount)
            .ToListAsync();
    }

    public async Task<string> GetProfileSummaryAsync(int surgeonId)
    {
        var surgeon = await GetByIdAsync(surgeonId);
        if (surgeon == null)
        {
            return "Surgeon not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль лікаря: {surgeon.FullName}");
        summaryBuilder.AppendLine($"Спеціалізація: {surgeon.Specialty}");
        summaryBuilder.AppendLine($"Кількість операцій: {surgeon.OperationCount}");
        summaryBuilder.AppendLine($"Кількість летальних операцій: {surgeon.FatalOperationCount}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(surgeonId);
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