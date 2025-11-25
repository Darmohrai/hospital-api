using System.Text;
using hospital_api.DTOs.Staff;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class NeurologistService : INeurologistService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public NeurologistService(
        IStaffRepository staffRepository,
        IEmploymentRepository employmentRepository,
        IHospitalRepository hospitalRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
        _hospitalRepository = hospitalRepository;
    }

    public async Task<IEnumerable<Neurologist>> GetAllNeurologistsAsync()
    {
        return await _staffRepository.GetAll()
            .OfType<Neurologist>()
            .ToListAsync();
    }

    public async Task<Neurologist?> GetNeurologistByIdAsync(int id)
    {
        return await _staffRepository.GetAll()
            .OfType<Neurologist>()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<ServiceResponse<Neurologist>> AddNeurologistToHospitalAsync(int? hospitalId,
        CreateNeurologistDto dto)
    {
        var neurologist = new Neurologist
        {
            FullName = dto.FullName,
            WorkExperienceYears = dto.WorkExperienceYears,
            Specialty = "Neurologist",
            AcademicDegree = dto.AcademicDegree,
            AcademicTitle = dto.AcademicTitle,
            ExtendedVacationDays = dto.ExtendedVacationDays,
        };

        await _staffRepository.AddAsync(neurologist);

        return ServiceResponse<Neurologist>.Success(neurologist);
    }

    public async Task UpdateNeurologistAsync(Neurologist neurologist)
    {
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

    public Task<int> GetNeurologistExtendedVacationDaysAsync(int neurologistId)
    {
        return _staffRepository.GetExtendedVacationDaysForDoctor(neurologistId);
    }

}