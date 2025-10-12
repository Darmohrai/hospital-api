using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IDentistService
{
    Task<IEnumerable<Dentist>> GetAllAsync();
    Task<Dentist?> GetByIdAsync(int id);
    Task<ServiceResponse<Dentist>> CreateAsync(Dentist dentist);
    Task<ServiceResponse<Dentist>> UpdateAsync(Dentist dentist);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
        
    Task<IEnumerable<Dentist>> GetByMinimumOperationCountAsync(int minOperationCount);
    Task<IEnumerable<Dentist>> GetByHazardPayCoefficientAsync(float minCoefficient);
        
    Task<string> GetSummaryAsync(int dentistId);
}