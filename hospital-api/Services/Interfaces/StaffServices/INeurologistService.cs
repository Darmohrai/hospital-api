using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface INeurologistService
{
    Task<IEnumerable<Neurologist>> GetAllNeurologistsAsync();
    Task<Neurologist?> GetNeurologistByIdAsync(int id);
    Task AddNeurologistAsync(Neurologist neurologist);
    Task UpdateNeurologistAsync(Neurologist neurologist);
    Task DeleteNeurologistAsync(int id);

    Task<IEnumerable<Neurologist>> GetNeurologistsByExtendedVacationDaysAsync(int minDays);
    Task<string> GetNeurologistProfileSummaryAsync(int neurologistId);
}
