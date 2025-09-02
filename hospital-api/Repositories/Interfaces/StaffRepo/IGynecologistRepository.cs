using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IGynecologistRepository : IRepository<Gynecologist>
{
    // Отримати всіх гінекологів, які зробили певну мінімальну кількість операцій
    Task<IEnumerable<Gynecologist>> GetByOperationCountAsync(int minOperationCount);

    // Отримати всіх гінекологів разом з їхніми операціями (для доступу до OperationCount та FatalOperationCount)
    Task<IEnumerable<Gynecologist>> GetAllWithOperationsAsync();
}