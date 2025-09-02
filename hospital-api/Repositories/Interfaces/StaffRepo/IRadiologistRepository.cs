using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IRadiologistRepository : IRepository<Radiologist>
{
    // Отримати радіологів за коефіцієнтом небезпеки
    Task<IEnumerable<Radiologist>> GetByHazardPayCoefficientAsync(float minCoefficient);

    // Отримати радіологів за кількістю додаткових днів відпустки
    Task<IEnumerable<Radiologist>> GetByExtendedVacationDaysAsync(int minDays);

    // Отримати радіологів, які відповідають обом критеріям
    Task<IEnumerable<Radiologist>> GetByHazardPayAndVacationAsync(float minCoefficient, int minDays);
}