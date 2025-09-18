using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IRadiologistService
{
    Task<IEnumerable<Radiologist>> GetAllRadiologistsAsync();
    Task<Radiologist?> GetRadiologistByIdAsync(int id);
    Task AddRadiologistAsync(Radiologist radiologist);
    Task UpdateRadiologistAsync(Radiologist radiologist);
    Task DeleteRadiologistAsync(int id);

    Task<IEnumerable<Radiologist>> GetByHazardPayCoefficientAsync(float minCoefficient);
    Task<IEnumerable<Radiologist>> GetByExtendedVacationDaysAsync(int minDays);
    Task<IEnumerable<Radiologist>> GetByHazardPayAndVacationAsync(float minCoefficient, int minDays);

    Task<string> GetRadiologistProfileSummaryAsync(int radiologistId);
}
