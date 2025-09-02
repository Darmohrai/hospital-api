using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class NeurologistRepository : GenericRepository<Neurologist>, INeurologistRepository
{
    public NeurologistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Neurologist>> GetByExtendedVacationDaysAsync(int minDays)
    {
        return await _dbSet
            .Where(n => n.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }
}