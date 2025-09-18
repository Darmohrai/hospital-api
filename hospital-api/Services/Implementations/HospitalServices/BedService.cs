using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using hospital_api.Services.Interfaces.HospitalServices;

namespace hospital_api.Services.Implementations.HospitalServices;

public class BedService : IBedService
{
    private readonly IBedRepository _bedRepository;

    public BedService(IBedRepository bedRepository)
    {
        _bedRepository = bedRepository;
    }

    // --- CRUD базові методи ---
    public async Task<Bed?> GetByIdAsync(int id)
    {
        return await _bedRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Bed>> GetAllAsync()
    {
        return await _bedRepository.GetAllAsync();
    }

    public async Task AddAsync(Bed bed)
    {
        await _bedRepository.AddAsync(bed);
    }

    public async Task UpdateAsync(Bed bed)
    {
        await _bedRepository.UpdateAsync(bed);
    }

    public async Task DeleteAsync(int id)
    {
        await _bedRepository.DeleteAsync(id);
    }

    // --- Специфічні методи ---
    public async Task<IEnumerable<Bed>> GetAvailableBedsAsync()
    {
        return await _bedRepository.GetAvailableBedsAsync();
    }

    public async Task<IEnumerable<Bed>> GetOccupiedBedsAsync()
    {
        return await _bedRepository.GetOccupiedBedsAsync();
    }

    public async Task<IEnumerable<Bed>> GetByRoomIdAsync(int roomId)
    {
        return await _bedRepository.GetByRoomIdAsync(roomId);
    }
}
