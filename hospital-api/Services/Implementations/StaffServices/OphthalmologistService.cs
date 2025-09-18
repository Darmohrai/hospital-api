using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class OphthalmologistService : IOphthalmologistService
{
    private readonly IOphthalmologistRepository _ophthalmologistRepository;

    public OphthalmologistService(IOphthalmologistRepository ophthalmologistRepository)
    {
        _ophthalmologistRepository = ophthalmologistRepository;
    }

    public async Task<IEnumerable<Ophthalmologist>> GetAllOphthalmologistsAsync()
    {
        return await _ophthalmologistRepository.GetAllAsync();
    }

    public async Task<Ophthalmologist?> GetOphthalmologistByIdAsync(int id)
    {
        return await _ophthalmologistRepository.GetByIdAsync(id);
    }

    public async Task AddOphthalmologistAsync(Ophthalmologist ophthalmologist)
    {
        if (string.IsNullOrWhiteSpace(ophthalmologist.FullName))
        {
            throw new ArgumentException("Ophthalmologist's full name is required.");
        }

        await _ophthalmologistRepository.AddAsync(ophthalmologist);
    }

    public async Task UpdateOphthalmologistAsync(Ophthalmologist ophthalmologist)
    {
        var existingOphthalmologist = await _ophthalmologistRepository.GetByIdAsync(ophthalmologist.Id);
        if (existingOphthalmologist == null)
        {
            throw new InvalidOperationException("Ophthalmologist not found.");
        }

        await _ophthalmologistRepository.UpdateAsync(ophthalmologist);
    }

    public async Task DeleteOphthalmologistAsync(int id)
    {
        await _ophthalmologistRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Ophthalmologist>> GetOphthalmologistsByExtendedVacationDaysAsync(int minDays)
    {
        return await _ophthalmologistRepository.GetByExtendedVacationDaysAsync(minDays);
    }

    public async Task<string> GetOphthalmologistProfileSummaryAsync(int ophthalmologistId)
    {
        var ophthalmologist = await _ophthalmologistRepository.GetByIdAsync(ophthalmologistId);

        if (ophthalmologist == null)
        {
            return "Ophthalmologist not found.";
        }

        return $"Профіль лікаря: {ophthalmologist.FullName}\n" +
               $"Спеціалізація: {ophthalmologist.Specialty}\n" +
               $"Дні додаткової відпустки: {ophthalmologist.ExtendedVacationDays}";
    }
}
