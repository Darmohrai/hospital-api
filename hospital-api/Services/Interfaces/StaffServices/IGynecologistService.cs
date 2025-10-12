using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IGynecologistService
{
    Task<IEnumerable<Gynecologist>> GetAllAsync();
    Task<Gynecologist?> GetByIdAsync(int id);
    Task<ServiceResponse<Gynecologist>> CreateAsync(Gynecologist gynecologist);
    Task<ServiceResponse<Gynecologist>> UpdateAsync(Gynecologist gynecologist);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
        
    Task<IEnumerable<Gynecologist>> GetByMinimumOperationCountAsync(int minOperations);
        
    Task<string> GetProfileSummaryAsync(int gynecologistId);
}
