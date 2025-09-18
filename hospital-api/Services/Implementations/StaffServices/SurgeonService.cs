using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;


public class SurgeonService : ISurgeonService
{
    private readonly ISurgeonRepository _surgeonRepository;

    public SurgeonService(ISurgeonRepository surgeonRepository)
    {
        _surgeonRepository = surgeonRepository;
    }

    public async Task<IEnumerable<Surgeon>> GetAllSurgeonsAsync()
    {
        return await _surgeonRepository.GetAllAsync();
    }

    public async Task<Surgeon?> GetSurgeonByIdAsync(int id)
    {
        return await _surgeonRepository.GetByIdAsync(id);
    }

    public async Task AddSurgeonAsync(Surgeon surgeon)
    {
        if (string.IsNullOrWhiteSpace(surgeon.FullName))
        {
            throw new ArgumentException("Surgeon's full name is required.");
        }

        await _surgeonRepository.AddAsync(surgeon);
    }

    public async Task UpdateSurgeonAsync(Surgeon surgeon)
    {
        var existingSurgeon = await _surgeonRepository.GetByIdAsync(surgeon.Id);
        if (existingSurgeon == null)
        {
            throw new InvalidOperationException("Surgeon not found.");
        }

        await _surgeonRepository.UpdateAsync(surgeon);
    }

    public async Task DeleteSurgeonAsync(int id)
    {
        await _surgeonRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Surgeon>> GetByOperationCountAsync(int minOperationCount)
    {
        return await _surgeonRepository.GetByOperationCountAsync(minOperationCount);
    }

    public async Task<IEnumerable<Surgeon>> GetAllWithOperationsAsync()
    {
        return await _surgeonRepository.GetAllWithOperationsAsync();
    }

    public async Task<string> GetSurgeonProfileSummaryAsync(int surgeonId)
    {
        var surgeon = await _surgeonRepository.GetByIdAsync(surgeonId);

        if (surgeon == null)
        {
            return "Surgeon not found.";
        }

        return $"Профіль лікаря: {surgeon.FullName}\n" +
               $"Спеціалізація: {surgeon.Specialty}\n" +
               $"Кількість операцій: {surgeon.OperationCount}";
    }
}
