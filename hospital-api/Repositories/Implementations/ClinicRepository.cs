using hospital_api.Data;
using hospital_api.Models.ClinicAggregate;
using hospital_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations;

public class ClinicRepository : GenericRepository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Переоприділяємо GetAllAsync для включення зв’язків
    public async Task<IEnumerable<Clinic>> GetAllAsync()
    {
        return await _context.Clinics
            .Include(c => c.Hospital)
            .Include(c => c.Employments)
            .Include(c => c.Patients)
            .ToListAsync();
    }

    // Переоприділяємо GetByIdAsync для включення зв’язків
    public async Task<Clinic?> GetByIdAsync(int id)
    {
        return await _context.Clinics
            .Include(c => c.Hospital)
            .Include(c => c.Employments)
            .Include(c => c.Patients)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Додатковий метод ExistsAsync
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Clinics.AnyAsync(c => c.Id == id);
    }
}