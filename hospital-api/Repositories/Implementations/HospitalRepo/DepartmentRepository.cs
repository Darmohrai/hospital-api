using hospital_api.Data;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.HospitalRepo;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Name == name);
    }

    public async Task<IEnumerable<Department>> GetBySpecializationAsync(string specialization)
    {
        return await _dbSet.Where(d => d.Specialization == specialization).ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByBuildingIdWithRoomsAsync(int buildingId)
    {
        return await _dbSet
            .Where(d => d.BuildingId == buildingId)
            .Include(d => d.Building)
            .Include(d => d.Rooms)
            .ToListAsync();
    }
}