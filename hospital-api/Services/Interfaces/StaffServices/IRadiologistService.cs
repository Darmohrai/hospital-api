using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IRadiologistService
{
    Task<IEnumerable<Radiologist>> GetAllAsync();
    Task<Radiologist?> GetByIdAsync(int id);
    Task<ServiceResponse<Radiologist>> CreateAsync(Radiologist radiologist);
    Task<ServiceResponse<Radiologist>> UpdateAsync(Radiologist radiologist);
    Task<ServiceResponse<bool>> DeleteAsync(int id);

    Task<IEnumerable<Radiologist>> GetByHazardPayCoefficientAsync(float minCoefficient);
    Task<IEnumerable<Radiologist>> GetByExtendedVacationDaysAsync(int minDays);
    Task<IEnumerable<Radiologist>> GetByHazardPayAndVacationAsync(float minCoefficient, int minDays);

    Task<string> GetProfileSummaryAsync(int radiologistId);

    Task<int> GetRadiologistExtendedVacationDaysAsync(int radiologistId);

    Task<float> GetRadiologistHazardPayCoefficientAsync(int radiologistId);
}