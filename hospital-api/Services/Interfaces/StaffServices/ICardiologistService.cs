using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface ICardiologistService
{
    Task<IEnumerable<Cardiologist>> GetAllAsync();
    Task<Cardiologist?> GetByIdAsync(int id);
    Task<ServiceResponse<Cardiologist>> CreateAsync(Cardiologist cardiologist);
    Task<ServiceResponse<Cardiologist>> UpdateAsync(Cardiologist cardiologist);
    Task<ServiceResponse<bool>> DeleteAsync(int id);
    Task<IEnumerable<Cardiologist>> GetByMinimumOperationCountAsync(int minOperations);
    Task<string> GetProfileSummaryAsync(int cardiologistId);
}