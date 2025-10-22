using hospital_api.Data;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.Tracking;

public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Appointment>> GetAllWithAssociationsAsync()
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Clinic)
            .Include(a => a.Hospital)
            .AsNoTracking()
            .ToListAsync();
    }
}