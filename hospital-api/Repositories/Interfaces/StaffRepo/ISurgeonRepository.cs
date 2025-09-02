using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface ISurgeonRepository : IRepository<Surgeon>
{
    // Отримати всіх хірургів, які зробили певну мінімальну кількість операцій
    Task<IEnumerable<Surgeon>> GetByOperationCountAsync(int minOperationCount);

    // Отримати всіх хірургів разом з їхніми операціями
    Task<IEnumerable<Surgeon>> GetAllWithOperationsAsync();
}