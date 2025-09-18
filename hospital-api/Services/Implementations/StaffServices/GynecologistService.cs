using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class GynecologistService : IGynecologistService
{
    private readonly IGynecologistRepository _gynecologistRepository;

    public GynecologistService(IGynecologistRepository gynecologistRepository)
    {
        _gynecologistRepository = gynecologistRepository;
    }

    public async Task<IEnumerable<Gynecologist>> GetAllGynecologistsAsync()
    {
        return await _gynecologistRepository.GetAllAsync();
    }

    public async Task<Gynecologist?> GetGynecologistByIdAsync(int id)
    {
        return await _gynecologistRepository.GetByIdAsync(id);
    }

    public async Task AddGynecologistAsync(Gynecologist gynecologist)
    {
        if (string.IsNullOrWhiteSpace(gynecologist.FullName))
        {
            throw new ArgumentException("Gynecologist's full name is required.");
        }

        await _gynecologistRepository.AddAsync(gynecologist);
    }

    public async Task UpdateGynecologistAsync(Gynecologist gynecologist)
    {
        var existingGynecologist = await _gynecologistRepository.GetByIdAsync(gynecologist.Id);
        if (existingGynecologist == null)
        {
            throw new InvalidOperationException("Gynecologist not found.");
        }

        await _gynecologistRepository.UpdateAsync(gynecologist);
    }

    public async Task DeleteGynecologistAsync(int id)
    {
        await _gynecologistRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Gynecologist>> GetTopSurgeonsByOperationsAsync(int minOperations)
    {
        return await _gynecologistRepository.GetByOperationCountAsync(minOperations);
    }

    public async Task<IEnumerable<Gynecologist>> GetAllWithOperationsAsync()
    {
        return await _gynecologistRepository.GetAllWithOperationsAsync();
    }

    public async Task<string> GetGynecologistProfileSummaryAsync(int gynecologistId)
    {
        var gynecologist = await _gynecologistRepository.GetByIdAsync(gynecologistId);

        if (gynecologist == null)
        {
            return "Gynecologist not found.";
        }

        return $"Профіль лікаря: {gynecologist.FullName}\n" +
               $"Спеціалізація: {gynecologist.Specialty}\n" +
               $"Кількість операцій: {gynecologist.Operations?.Count ?? 0}";
    }
}
