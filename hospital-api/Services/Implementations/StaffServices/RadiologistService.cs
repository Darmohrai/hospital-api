using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class RadiologistService : IRadiologistService
{
    private readonly IRadiologistRepository _radiologistRepository;

    public RadiologistService(IRadiologistRepository radiologistRepository)
    {
        _radiologistRepository = radiologistRepository;
    }

    public async Task<IEnumerable<Radiologist>> GetAllRadiologistsAsync()
    {
        return await _radiologistRepository.GetAllAsync();
    }

    public async Task<Radiologist?> GetRadiologistByIdAsync(int id)
    {
        return await _radiologistRepository.GetByIdAsync(id);
    }

    public async Task AddRadiologistAsync(Radiologist radiologist)
    {
        if (string.IsNullOrWhiteSpace(radiologist.FullName))
        {
            throw new ArgumentException("Radiologist's full name is required.");
        }

        await _radiologistRepository.AddAsync(radiologist);
    }

    public async Task UpdateRadiologistAsync(Radiologist radiologist)
    {
        var existingRadiologist = await _radiologistRepository.GetByIdAsync(radiologist.Id);
        if (existingRadiologist == null)
        {
            throw new InvalidOperationException("Radiologist not found.");
        }

        await _radiologistRepository.UpdateAsync(radiologist);
    }

    public async Task DeleteRadiologistAsync(int id)
    {
        await _radiologistRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Radiologist>> GetByHazardPayCoefficientAsync(float minCoefficient)
    {
        return await _radiologistRepository.GetByHazardPayCoefficientAsync(minCoefficient);
    }

    public async Task<IEnumerable<Radiologist>> GetByExtendedVacationDaysAsync(int minDays)
    {
        return await _radiologistRepository.GetByExtendedVacationDaysAsync(minDays);
    }

    public async Task<IEnumerable<Radiologist>> GetByHazardPayAndVacationAsync(float minCoefficient, int minDays)
    {
        return await _radiologistRepository.GetByHazardPayAndVacationAsync(minCoefficient, minDays);
    }

    public async Task<string> GetRadiologistProfileSummaryAsync(int radiologistId)
    {
        var radiologist = await _radiologistRepository.GetByIdAsync(radiologistId);

        if (radiologist == null)
        {
            return "Radiologist not found.";
        }

        return $"Профіль лікаря: {radiologist.FullName}\n" +
               $"Спеціалізація: {radiologist.Specialty}\n" +
               $"Коефіцієнт шкідливості: {radiologist.HazardPayCoefficient}\n" +
               $"Дні додаткової відпустки: {radiologist.ExtendedVacationDays}";
    }
}
