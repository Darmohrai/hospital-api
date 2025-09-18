using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class NeurologistService : INeurologistService
{
    private readonly INeurologistRepository _neurologistRepository;

    public NeurologistService(INeurologistRepository neurologistRepository)
    {
        _neurologistRepository = neurologistRepository;
    }

    public async Task<IEnumerable<Neurologist>> GetAllNeurologistsAsync()
    {
        return await _neurologistRepository.GetAllAsync();
    }

    public async Task<Neurologist?> GetNeurologistByIdAsync(int id)
    {
        return await _neurologistRepository.GetByIdAsync(id);
    }

    public async Task AddNeurologistAsync(Neurologist neurologist)
    {
        if (string.IsNullOrWhiteSpace(neurologist.FullName))
        {
            throw new ArgumentException("Neurologist's full name is required.");
        }

        await _neurologistRepository.AddAsync(neurologist);
    }

    public async Task UpdateNeurologistAsync(Neurologist neurologist)
    {
        var existingNeurologist = await _neurologistRepository.GetByIdAsync(neurologist.Id);
        if (existingNeurologist == null)
        {
            throw new InvalidOperationException("Neurologist not found.");
        }

        await _neurologistRepository.UpdateAsync(neurologist);
    }

    public async Task DeleteNeurologistAsync(int id)
    {
        await _neurologistRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Neurologist>> GetNeurologistsByExtendedVacationDaysAsync(int minDays)
    {
        return await _neurologistRepository.GetByExtendedVacationDaysAsync(minDays);
    }

    public async Task<string> GetNeurologistProfileSummaryAsync(int neurologistId)
    {
        var neurologist = await _neurologistRepository.GetByIdAsync(neurologistId);

        if (neurologist == null)
        {
            return "Neurologist not found.";
        }

        return $"Профіль лікаря: {neurologist.FullName}\n" +
               $"Спеціалізація: {neurologist.Specialty}\n" +
               $"Дні додаткової відпустки: {neurologist.ExtendedVacationDays}";
    }
}
