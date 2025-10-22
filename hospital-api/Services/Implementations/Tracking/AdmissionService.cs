using hospital_api.DTOs.Tracking;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using hospital_api.Services.Interfaces.Tracking;

namespace hospital_api.Services.Implementations.Tracking;

public class AdmissionService : IAdmissionService
{
    private readonly IAdmissionRepository _admissionRepo;

    public AdmissionService(IAdmissionRepository admissionRepo)
    {
        _admissionRepo = admissionRepo;
    }

    public Task<Admission?> GetByIdAsync(int id) => _admissionRepo.GetByIdAsync(id);
    public Task<IEnumerable<Admission>> GetAllAsync() => _admissionRepo.GetAllAsync();

    public async Task<Admission> CreateAsync(CreateAdmissionDto dto)
    {
        // ВАЛІДАЦІЯ: Перевіряємо, чи пацієнт вже не госпіталізований
        var activeAdmission = (await _admissionRepo.FindByConditionAsync(
            a => a.PatientId == dto.PatientId && a.DischargeDate == null
        )).FirstOrDefault();

        if (activeAdmission != null)
        {
            throw new InvalidOperationException($"Пацієнт (ID: {dto.PatientId}) вже госпіталізований (AdmissionID: {activeAdmission.Id}). Спочатку потрібно його виписати.");
        }

        var admission = new Admission
        {
            AdmissionDate = dto.AdmissionDate,
            PatientId = dto.PatientId,
            HospitalId = dto.HospitalId,
            AttendingDoctorId = dto.AttendingDoctorId,
            DepartmentId = dto.DepartmentId,
            DischargeDate = null // Явно вказуємо, що пацієнт не виписаний
        };

        await _admissionRepo.AddAsync(admission);
        return admission;
    }

    public async Task<Admission?> DischargeAsync(int admissionId, DateTime dischargeDate)
    {
        var admission = await _admissionRepo.GetByIdAsync(admissionId);
        if (admission == null)
            return null; // Не знайдено
        
        if (admission.DischargeDate != null)
            throw new InvalidOperationException("Цей пацієнт вже виписаний.");
        
        if (dischargeDate.Date < admission.AdmissionDate.Date)
            throw new ArgumentException("Дата виписки не може бути раніше дати госпіталізації.");

        admission.DischargeDate = dischargeDate;
        await _admissionRepo.UpdateAsync(admission);
        return admission;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _admissionRepo.GetByIdAsync(id);
        if (existing == null)
            return false;
        
        // Додаткова логіка: не давати видаляти активні госпіталізації?
        if (existing.DischargeDate == null)
            throw new InvalidOperationException("Неможливо видалити активну госпіталізацію. Спочатку випишіть пацієнта.");

        await _admissionRepo.DeleteAsync(id);
        return true;
    }
}