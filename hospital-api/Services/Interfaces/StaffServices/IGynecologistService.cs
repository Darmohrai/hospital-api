using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IGynecologistService
{
    Task<IEnumerable<Gynecologist>> GetAllGynecologistsAsync();
    Task<Gynecologist?> GetGynecologistByIdAsync(int id);
    Task AddGynecologistAsync(Gynecologist gynecologist);
    Task UpdateGynecologistAsync(Gynecologist gynecologist);
    Task DeleteGynecologistAsync(int id);

    Task<IEnumerable<Gynecologist>> GetTopSurgeonsByOperationsAsync(int minOperations);
    Task<IEnumerable<Gynecologist>> GetAllWithOperationsAsync();

    Task<string> GetGynecologistProfileSummaryAsync(int gynecologistId);
}
