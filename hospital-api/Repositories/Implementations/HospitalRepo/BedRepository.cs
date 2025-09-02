using hospital_api.Data;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.HospitalRepo;

public class BedRepository : GenericRepository<Bed>, IBedRepository
{
    public BedRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Bed>> GetAvailableBedsAsync()
    {
        return await _dbSet
            .Where(b => !b.IsOccupied)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bed>> GetOccupiedBedsAsync()
    {
        return await _dbSet
            .Where(b => b.IsOccupied)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bed>> GetByRoomIdAsync(int roomId)
    {
        return await _dbSet
            .Where(b => b.RoomId == roomId)
            .Include(b => b.Room) // "Жадібне" завантаження пов'язаної кімнати
            .ToListAsync();
    }
}