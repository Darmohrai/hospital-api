using System.Text;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class RadiologistService : IRadiologistService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public RadiologistService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Radiologist>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Radiologist>().ToListAsync();
    }

    public async Task<Radiologist?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Radiologist;
    }

    public async Task<ServiceResponse<Radiologist>> CreateAsync(Radiologist radiologist)
    {
        if (string.IsNullOrWhiteSpace(radiologist.FullName))
        {
            return ServiceResponse<Radiologist>.Fail("Radiologist's full name is required.");
        }

        radiologist.Specialty = "Radiologist";
        await _staffRepository.AddAsync(radiologist);
        return ServiceResponse<Radiologist>.Success(radiologist);
    }

    public async Task<ServiceResponse<Radiologist>> UpdateAsync(Radiologist radiologist)
    {
        var existing = await GetByIdAsync(radiologist.Id);
        if (existing == null)
        {
            return ServiceResponse<Radiologist>.Fail("Radiologist not found.");
        }

        await _staffRepository.UpdateAsync(radiologist);
        return ServiceResponse<Radiologist>.Success(radiologist);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null)
        {
            return ServiceResponse<bool>.Fail("Radiologist not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Radiologist>> GetByHazardPayCoefficientAsync(float minCoefficient)
    {
        return await _staffRepository.GetAll()
            .OfType<Radiologist>()
            .Where(r => r.HazardPayCoefficient >= minCoefficient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Radiologist>> GetByExtendedVacationDaysAsync(int minDays)
    {
        return await _staffRepository.GetAll()
            .OfType<Radiologist>()
            .Where(r => r.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }

    public async Task<IEnumerable<Radiologist>> GetByHazardPayAndVacationAsync(float minCoefficient, int minDays)
    {
        return await _staffRepository.GetAll()
            .OfType<Radiologist>()
            .Where(r => r.HazardPayCoefficient >= minCoefficient && r.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }

    public async Task<string> GetProfileSummaryAsync(int radiologistId)
    {
        var radiologist = await GetByIdAsync(radiologistId);
        if (radiologist == null)
        {
            return "Radiologist not found.";
        }

        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Профіль лікаря: {radiologist.FullName}");
        summaryBuilder.AppendLine($"Спеціалізація: {radiologist.Specialty}");
        summaryBuilder.AppendLine($"Коефіцієнт шкідливості: {radiologist.HazardPayCoefficient}");
        summaryBuilder.AppendLine($"Дні додаткової відпустки: {radiologist.ExtendedVacationDays}");

        var employments = await _employmentRepository.GetEmploymentsByStaffIdAsync(radiologistId);
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
    
    public Task<int> GetRadiologistExtendedVacationDaysAsync(int radiologistId)
    {
        return _staffRepository.GetExtendedVacationDaysForDoctor(radiologistId);
    }
    
    public Task<float> GetRadiologistHazardPayCoefficientAsync(int radiologistId)
    {
        return _staffRepository.GetHazardPayCoefficientForDoctor(radiologistId);
    }
}