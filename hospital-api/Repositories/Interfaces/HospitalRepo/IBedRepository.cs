using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Repositories.Interfaces.HospitalRepo;

public interface IBedRepository : IRepository<Bed>
{
    Task<IEnumerable<Bed>> GetAvailableBedsAsync();

    Task<IEnumerable<Bed>> GetOccupiedBedsAsync();

    Task<IEnumerable<Bed>> GetByRoomIdAsync(int roomId);
}