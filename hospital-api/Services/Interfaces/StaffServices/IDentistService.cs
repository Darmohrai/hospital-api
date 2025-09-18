using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IDentistService
{
    // Базові операції CRUD
    Task<IEnumerable<Dentist>> GetAllDentistsAsync();
    Task<Dentist?> GetDentistByIdAsync(int id);
    Task AddDentistAsync(Dentist dentist);
    Task UpdateDentistAsync(Dentist dentist);
    Task DeleteDentistAsync(int id);

    // Спеціалізовані методи, що використовують бізнес-логіку
    Task<IEnumerable<Dentist>> GetTopPerformingDentistsAsync(int minOperationCount);
    Task<IEnumerable<Dentist>> GetDentistsWithHighHazardPayAsync(float minCoefficient);
    Task<string> GetDentistSummaryAsync(int dentistId);
}