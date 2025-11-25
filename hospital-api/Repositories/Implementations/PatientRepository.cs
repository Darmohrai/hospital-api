using hospital_api.Data;
using hospital_api.Models.PatientAggregate;
using hospital_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Patient>> GetByFullNameAsync(string fullName)
    {
        return await _dbSet
            .Where(p => p.FullName.Contains(fullName))
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetByHealthStatusAsync(string status)
    {
        return await _dbSet
            .Where(p => p.HealthStatus == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetByClinicIdAsync(int clinicId)
    {
        return await _dbSet
            .Where(p => p.ClinicId == clinicId)
            .Include(p => p.Clinic)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _dbSet
            .Where(p => p.HospitalId == hospitalId)
            .Include(p => p.Hospital)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetByAssignedDoctorIdAsync(int doctorId)
    {
        return await _dbSet
            .Where(p => p.AssignedDoctorId == doctorId)
            .Include(p => p.AssignedDoctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetAllWithAssociationsAsync()
    {
        return await _context.Patients
            .Include(p => p.Clinic)
            .Include(p => p.Hospital)
            .Include(p => p.AssignedDoctor)
            .Include(p => p.Bed)
            .ThenInclude(b => b.Room)
            .ThenInclude(r => r.Department)
            .AsNoTracking()
            .ToListAsync();
    }
}