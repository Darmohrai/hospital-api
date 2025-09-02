using hospital_api.Data;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.HospitalRepo;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Room?> GetByNumberAsync(string roomNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Number == roomNumber);
    }

    public async Task<IEnumerable<Room>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _dbSet
            .Where(r => r.DepartmentId == departmentId)
            .Include(r => r.Department)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Room>> GetByCapacityAsync(int capacity)
    {
        return await _dbSet
            .Where(r => r.Capacity >= capacity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetAllWithBedsAndDepartmentAsync()
    {
        return await _dbSet
            .Include(r => r.Beds)
            .Include(r => r.Department)
            .ToListAsync();
    }
}