using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class DentistRepository : GenericRepository<Dentist>, IDentistRepository
{
    public DentistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Dentist>> GetByHazardPayCoefficientAsync(float minCoefficient)
    {
        return await _dbSet.Where(d => d.HazardPayCoefficient >= minCoefficient).ToListAsync();
    }

    public async Task<IEnumerable<Dentist>> GetByOperationCountAsync(int minOperationCount)
    {
        return await _dbSet
            .Include(d => d.Operations)
            .Where(d => d.Operations.Count >= minOperationCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<Dentist>> GetAllWithOperationsAsync()
    {
        return await _dbSet
            .Include(d => d.Operations)
            .ToListAsync();
    }
}