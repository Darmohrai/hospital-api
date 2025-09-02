using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class SurgeonRepository : GenericRepository<Surgeon>, ISurgeonRepository
{
    public SurgeonRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Surgeon>> GetByOperationCountAsync(int minOperationCount)
    {
        return await _dbSet
            .Include(s => s.Operations)
            .Where(s => s.Operations.Count >= minOperationCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Surgeon>> GetAllWithOperationsAsync()
    {
        return await _dbSet
            .Include(s => s.Operations)
            .ToListAsync();
    }
}