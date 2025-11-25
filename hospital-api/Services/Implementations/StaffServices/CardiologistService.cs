using System.Text;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class CardiologistService : ICardiologistService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public CardiologistService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Cardiologist>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Cardiologist>().ToListAsync();
    }

    public async Task<Cardiologist?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Cardiologist;
    }

    public async Task<ServiceResponse<Cardiologist>> CreateAsync(Cardiologist cardiologist)
    {
        if (string.IsNullOrWhiteSpace(cardiologist.FullName))
        {
            return ServiceResponse<Cardiologist>.Fail("Cardiologist's full name is required.");
        }

        cardiologist.Specialty = "Cardiologist";
        await _staffRepository.AddAsync(cardiologist);
        return ServiceResponse<Cardiologist>.Success(cardiologist);
    }

    public async Task<ServiceResponse<Cardiologist>> UpdateAsync(Cardiologist cardiologist)
    {
        var existing = await GetByIdAsync(cardiologist.Id);
        if (existing == null)
        {
            return ServiceResponse<Cardiologist>.Fail("Cardiologist not found.");
        }

        await _staffRepository.UpdateAsync(cardiologist);
        return ServiceResponse<Cardiologist>.Success(cardiologist);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResponse<bool>.Fail("Cardiologist not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Cardiologist>> GetByMinimumOperationCountAsync(int minOperations)
    {
        return await _staffRepository.GetAll()
            .OfType<Cardiologist>()
            .Where(c => c.OperationCount >= minOperations)
            .ToListAsync();
    }

    public async Task<string> GetProfileSummaryAsync(int cardiologistId)
    {
        var cardiologist = await GetByIdAsync(cardiologistId);
        if (cardiologist == null)
        {
            return "Cardiologist not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль лікаря: {cardiologist.FullName}");
        summaryBuilder.AppendLine($"Спеціалізація: {cardiologist.Specialty}");
        summaryBuilder.AppendLine($"Кількість операцій: {cardiologist.OperationCount}");
        summaryBuilder.AppendLine($"Кількість летальних операцій: {cardiologist.FatalOperationCount}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(cardiologistId);
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