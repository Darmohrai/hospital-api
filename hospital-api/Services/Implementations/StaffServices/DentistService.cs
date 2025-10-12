using System.Text;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class DentistService : IDentistService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public DentistService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Dentist>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Dentist>().ToListAsync();
    }

    public async Task<Dentist?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Dentist;
    }

    public async Task<ServiceResponse<Dentist>> CreateAsync(Dentist dentist)
    {
        if (dentist.HazardPayCoefficient < 0)
        {
            return ServiceResponse<Dentist>.Fail("Hazard pay coefficient cannot be negative.");
        }

        dentist.Specialty = "Dentist"; // Встановлюємо спеціальність
        await _staffRepository.AddAsync(dentist);
        return ServiceResponse<Dentist>.Success(dentist);
    }

    public async Task<ServiceResponse<Dentist>> UpdateAsync(Dentist dentist)
    {
        var existing = await GetByIdAsync(dentist.Id);
        if (existing == null)
        {
            return ServiceResponse<Dentist>.Fail("Dentist not found.");
        }

        await _staffRepository.UpdateAsync(dentist);
        return ServiceResponse<Dentist>.Success(dentist);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResponse<bool>.Fail("Dentist not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Dentist>> GetByMinimumOperationCountAsync(int minOperationCount)
    {
        return await _staffRepository.GetAll()
            .OfType<Dentist>()
            .Where(d => d.OperationCount >= minOperationCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Dentist>> GetByHazardPayCoefficientAsync(float minCoefficient)
    {
        return await _staffRepository.GetAll()
            .OfType<Dentist>()
            .Where(d => d.HazardPayCoefficient >= minCoefficient)
            .ToListAsync();
    }

    public async Task<string> GetSummaryAsync(int dentistId)
    {
        var dentist = await GetByIdAsync(dentistId);
        if (dentist == null)
        {
            return "Dentist not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль: {dentist.FullName}");
        summaryBuilder.AppendLine($"Спеціальність: {dentist.Specialty}");
        summaryBuilder.AppendLine($"Кількість операцій: {dentist.OperationCount}");
        summaryBuilder.AppendLine($"Летальних операцій: {dentist.FatalOperationCount}");
        summaryBuilder.AppendLine($"Коефіцієнт шкідливості: {dentist.HazardPayCoefficient}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(dentistId);
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