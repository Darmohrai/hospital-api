using System.Text;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class OphthalmologistService : IOphthalmologistService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public OphthalmologistService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Ophthalmologist>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Ophthalmologist>().ToListAsync();
    }

    public async Task<Ophthalmologist?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Ophthalmologist;
    }

    public async Task<ServiceResponse<Ophthalmologist>> CreateAsync(Ophthalmologist ophthalmologist)
    {
        if (string.IsNullOrWhiteSpace(ophthalmologist.FullName))
        {
            return ServiceResponse<Ophthalmologist>.Fail("Ophthalmologist's full name is required.");
        }

        ophthalmologist.Specialty = "Ophthalmologist";
        await _staffRepository.AddAsync(ophthalmologist);
        return ServiceResponse<Ophthalmologist>.Success(ophthalmologist);
    }

    public async Task<ServiceResponse<Ophthalmologist>> UpdateAsync(Ophthalmologist ophthalmologist)
    {
        var existing = await GetByIdAsync(ophthalmologist.Id);
        if (existing == null)
        {
            return ServiceResponse<Ophthalmologist>.Fail("Ophthalmologist not found.");
        }

        await _staffRepository.UpdateAsync(ophthalmologist);
        return ServiceResponse<Ophthalmologist>.Success(ophthalmologist);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResponse<bool>.Fail("Ophthalmologist not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Ophthalmologist>> GetByExtendedVacationDaysAsync(int minDays)
    {
        return await _staffRepository.GetAll()
            .OfType<Ophthalmologist>()
            .Where(o => o.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }

    public async Task<string> GetProfileSummaryAsync(int ophthalmologistId)
    {
        var ophthalmologist = await GetByIdAsync(ophthalmologistId);
        if (ophthalmologist == null)
        {
            return "Ophthalmologist not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль лікаря: {ophthalmologist.FullName}");
        summaryBuilder.AppendLine($"Спеціалізація: {ophthalmologist.Specialty}");
        summaryBuilder.AppendLine($"Дні додаткової відпустки: {ophthalmologist.ExtendedVacationDays}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(ophthalmologistId);
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