using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class RadiologistRepository : GenericRepository<Radiologist>, IRadiologistRepository
{
    public RadiologistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Radiologist>> GetByHazardPayCoefficientAsync(float minCoefficient)
    {
        return await _dbSet
            .Where(r => r.HazardPayCoefficient >= minCoefficient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Radiologist>> GetByExtendedVacationDaysAsync(int minDays)
    {
        return await _dbSet
            .Where(r => r.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }

    public async Task<IEnumerable<Radiologist>> GetByHazardPayAndVacationAsync(float minCoefficient, int minDays)
    {
        return await _dbSet
            .Where(r => r.HazardPayCoefficient >= minCoefficient && r.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }
}