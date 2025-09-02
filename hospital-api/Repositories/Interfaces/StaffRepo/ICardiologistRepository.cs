using hospital_api.Models.StaffAggregate.DoctorAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface ICardiologistRepository : IRepository<Cardiologist>
{
    Task<IEnumerable<Cardiologist>> GetByOperationCountAsync(int minOperations);
    Task<IEnumerable<Cardiologist>> GetByFatalOperationCountAsync(int minFatalOperations);
}