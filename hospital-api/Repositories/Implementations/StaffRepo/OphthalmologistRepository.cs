using hospital_api.Data;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class OphthalmologistRepository : GenericRepository<Ophthalmologist>, IOphthalmologistRepository
{
    public OphthalmologistRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Ophthalmologist>> GetByExtendedVacationDaysAsync(int minDays)
    {
        return await _dbSet
            .Where(o => o.ExtendedVacationDays >= minDays)
            .ToListAsync();
    }
}