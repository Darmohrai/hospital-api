using hospital_api.Data;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class SupportStaffRepository : GenericRepository<SupportStaff>, ISupportStaffRepository
{
    public SupportStaffRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role)
    {
        return await _dbSet.Where(s => s.Role == role).ToListAsync();
    }

    public async Task<IEnumerable<SupportStaff>> GetByClinicIdAndRoleAsync(int clinicId, SupportRole role)
    {
        return await _dbSet
            .Where(s => s.ClinicId == clinicId && s.Role == role)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportStaff>> GetByHospitalIdAndRoleAsync(int hospitalId, SupportRole role)
    {
        return await _dbSet
            .Where(s => s.HospitalId == hospitalId && s.Role == role)
            .ToListAsync();
    }
}