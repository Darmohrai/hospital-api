using hospital_api.Data;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class StaffRepository : GenericRepository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Staff>> GetByClinicIdAsync(int clinicId)
    {
        return await _dbSet.Where(s => s.ClinicId == clinicId).ToListAsync();
    }

    public async Task<IEnumerable<Staff>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _dbSet.Where(s => s.HospitalId == hospitalId).ToListAsync();
    }
}