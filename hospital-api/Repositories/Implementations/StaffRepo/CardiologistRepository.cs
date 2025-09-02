using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class CardiologistRepository : GenericRepository<Cardiologist>, ICardiologistRepository
{
    public CardiologistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Cardiologist>> GetByOperationCountAsync(int minOperations)
    {
        return await _dbSet
            .Where(c => c.Operations.Count >= minOperations)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cardiologist>> GetByFatalOperationCountAsync(int minFatalOperations)
    {
        return await _dbSet
            .Where(c => c.Operations.Count(op => op.IsFatal) >= minFatalOperations)
            .ToListAsync();
    }
}