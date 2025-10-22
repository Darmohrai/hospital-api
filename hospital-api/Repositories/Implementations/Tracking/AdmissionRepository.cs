using hospital_api.Data;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.Tracking;

public class AdmissionRepository : GenericRepository<Admission>, IAdmissionRepository
{
    public AdmissionRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Admission>> GetAllWithAssociationsAsync()
    {
        return await _context.Admissions
            .Include(a => a.Patient)
            .Include(a => a.AttendingDoctor)
            .AsNoTracking()
            .ToListAsync();
    }
}