using hospital_api.Data;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.HospitalRepo;

public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
{
    public BuildingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Building?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(b => b.Name == name);
    }

    public async Task<IEnumerable<Building>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _dbSet
            .Where(b => b.HospitalId == hospitalId)
            .Include(b => b.Hospital)
            .ToListAsync();
    }

    public async Task<IEnumerable<Building>> GetAllWithDepartmentsAsync()
    {
        return await _dbSet
            .Include(b => b.Departments)
            .ToListAsync();
    }
}