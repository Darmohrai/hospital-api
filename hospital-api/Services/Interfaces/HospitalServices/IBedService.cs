using hospital_api.Models.HospitalAggregate;

namespace hospital_api.Services.Interfaces.HospitalServices;

public interface IBedService
{
    Task<Bed?> GetByIdAsync(int id);
    Task<IEnumerable<Bed>> GetAllAsync();
    Task AddAsync(Bed bed);
    Task UpdateAsync(Bed bed);
    Task DeleteAsync(int id);

    Task<IEnumerable<Bed>> GetAvailableBedsAsync();
    Task<IEnumerable<Bed>> GetOccupiedBedsAsync();
    Task<IEnumerable<Bed>> GetByRoomIdAsync(int roomId);
}
