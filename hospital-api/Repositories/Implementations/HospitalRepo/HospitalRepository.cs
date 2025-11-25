using hospital_api.Data;
using Microsoft.EntityFrameworkCore;
using hospital_api.Models.HospitalAggregate;
using hospital_api.Repositories.Interfaces.HospitalRepo;

namespace hospital_api.Repositories.Implementations.HospitalRepo;

public class HospitalRepository : GenericRepository<Hospital>, IHospitalRepository
{
    public HospitalRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Hospital?> GetByIdAsync(int id)
    {
        return await _context.Hospitals
            .Include(h => h.Buildings)
            .Include(h => h.Departments)
            .FirstOrDefaultAsync(h => h.Id == id);
    }
}