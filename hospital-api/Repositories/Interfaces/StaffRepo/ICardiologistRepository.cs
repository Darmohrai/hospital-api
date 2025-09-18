using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface ICardiologistRepository : IRepository<Cardiologist>
{
    // Отримати всіх кардіологів з певною мінімальною кількістю операцій
    Task<IEnumerable<Cardiologist>> GetByOperationCountAsync(int minOperationCount);

    // Отримати всіх кардіологів з певною мінімальною кількістю летальних операцій
    Task<IEnumerable<Cardiologist>> GetByFatalOperationCountAsync(int minFatalOperations);

    // Отримати всіх кардіологів разом з їхніми операціями
    Task<IEnumerable<Cardiologist>> GetAllWithOperationsAsync();
}