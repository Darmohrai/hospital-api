using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IRoomRepository : IRepository<Room>
{
    Task<Room?> GetByNumberAsync(string roomNumber);

    Task<IEnumerable<Room>> GetByDepartmentIdAsync(int departmentId);
    
    Task<IEnumerable<Room>> GetByCapacityAsync(int capacity);

    Task<IEnumerable<Room>> GetAllWithBedsAndDepartmentAsync();
}