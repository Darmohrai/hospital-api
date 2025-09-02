using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IDentistRepository : IRepository<Dentist>
{
    // Отримати всіх дантистів з коефіцієнтом небезпеки, що перевищує задане значення
    Task<IEnumerable<Dentist>> GetByHazardPayCoefficientAsync(float minCoefficient);

    // Отримати дантистів за кількістю проведених операцій
    Task<IEnumerable<Dentist>> GetByOperationCountAsync(int minOperationCount);
    
    // Отримати всіх дантистів з їхніми операціями (щоб мати доступ до властивостей OperationCount та FatalOperationCount)
    Task<IEnumerable<Dentist>> GetAllWithOperationsAsync();
}