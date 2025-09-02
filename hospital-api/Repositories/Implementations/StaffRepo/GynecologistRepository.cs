using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class GynecologistRepository : GenericRepository<Gynecologist>, IGynecologistRepository
{
    public GynecologistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Gynecologist>> GetByOperationCountAsync(int minOperationCount)
    {
        return await _dbSet
            .Include(g => g.Operations)
            .Where(g => g.Operations.Count >= minOperationCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Gynecologist>> GetAllWithOperationsAsync()
    {
        return await _dbSet
            .Include(g => g.Operations)
            .ToListAsync();
    }
}