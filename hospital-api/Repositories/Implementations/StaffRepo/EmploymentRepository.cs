using hospital_api.Data;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class EmploymentRepository : GenericRepository<Employment>, IEmploymentRepository
{
    public EmploymentRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Employment>> GetEmploymentsByStaffIdAsync(int staffId)
    {
        return await _context.Employments
            .Where(e => e.StaffId == staffId)
            .Include(e => e.Hospital)
            .Include(e => e.Clinic)
            .ToListAsync();
    }

    public async Task<IEnumerable<Employment>> GetEmploymentsByHospitalIdAsync(int hospitalId)
    {
        return await _context.Employments
            .Where(e => e.HospitalId == hospitalId)
            .Include(e => e.Staff)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Employment>> GetEmploymentsByClinicIdAsync(int clinicId)
    {
        return await _context.Employments
            .Where(e => e.ClinicId == clinicId)
            .Include(e => e.Staff)
            .ToListAsync();
    }
}