using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface ISurgeonService
{
    Task<IEnumerable<Surgeon>> GetAllAsync();
    Task<Surgeon?> GetByIdAsync(int id);
    Task<ServiceResponse<Surgeon>> CreateAsync(Surgeon surgeon);
    Task<ServiceResponse<Surgeon>> UpdateAsync(Surgeon surgeon);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
        
    Task<IEnumerable<Surgeon>> GetByMinimumOperationCountAsync(int minOperationCount);
        
    Task<string> GetProfileSummaryAsync(int surgeonId);
}
