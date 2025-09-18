using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface ISurgeonService
{
    Task<IEnumerable<Surgeon>> GetAllSurgeonsAsync();
    Task<Surgeon?> GetSurgeonByIdAsync(int id);
    Task AddSurgeonAsync(Surgeon surgeon);
    Task UpdateSurgeonAsync(Surgeon surgeon);
    Task DeleteSurgeonAsync(int id);

    Task<IEnumerable<Surgeon>> GetByOperationCountAsync(int minOperationCount);
    Task<IEnumerable<Surgeon>> GetAllWithOperationsAsync();
    Task<string> GetSurgeonProfileSummaryAsync(int surgeonId);
}
