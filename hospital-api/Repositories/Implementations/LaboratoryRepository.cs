using hospital_api.Data;
using hospital_api.Models.LaboratoryAggregate;
using hospital_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations;

public class LaboratoryRepository : GenericRepository<Laboratory>, ILaboratoryRepository
{
    public LaboratoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Laboratory>> GetByProfileAsync(string profile)
    {
        return await _dbSet
            .Where(l => l.Profile.Contains(profile))
            .ToListAsync();
    }

    public async Task<IEnumerable<Laboratory>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _dbSet
            .Where(l => l.Hospitals.Any(h => h.Id == hospitalId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Laboratory>> GetByClinicIdAsync(int clinicId)
    {
        return await _dbSet
            .Where(l => l.Clinics.Any(c => c.Id == clinicId))
            .ToListAsync();
    }

    public async Task<Laboratory?> GetByNameWithAssociationsAsync(string name)
    {
        return await _dbSet
            .Where(l => l.Name == name)
            .Include(l => l.Hospitals)
            .Include(l => l.Clinics)
            .FirstOrDefaultAsync();
    }
}