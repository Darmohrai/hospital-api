using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface ICardiologistService
{
    // Базові операції
    Task<IEnumerable<Cardiologist>> GetAllCardiologistsAsync();
    Task<Cardiologist?> GetCardiologistByIdAsync(int id);
    Task AddCardiologistAsync(Cardiologist cardiologist);
    Task UpdateCardiologistAsync(Cardiologist cardiologist);
    Task DeleteCardiologistAsync(int id);

    // Спеціалізовані методи, що використовують бізнес-логіку
    Task<IEnumerable<Cardiologist>> GetTopSurgeonsByOperationsAsync(int minOperations);
    Task<IEnumerable<Cardiologist>> GetCardiologistsWithFatalOperationsAsync();
    
    // Приклад методу, що поєднує дані з кількох джерел або виконує обчислення
    Task<string> GetCardiologistProfileSummaryAsync(int cardiologistId);
}