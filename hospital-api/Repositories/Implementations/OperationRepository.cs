using hospital_api.Data;
using hospital_api.Models.OperationsAggregate;
using hospital_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations;

public class OperationRepository : GenericRepository<Operation>, IOperationRepository
{
    public OperationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Operation>> GetByPatientIdAsync(int patientId)
    {
        return await _dbSet
            .Where(op => op.PatientId == patientId)
            .Include(op => op.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<Operation>> GetByDoctorIdAsync(int doctorId)
    {
        return await _dbSet
            .Where(op => op.DoctorId == doctorId)
            .Include(op => op.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Operation>> GetByHospitalIdAsync(int hospitalId)
    {
        return await _dbSet
            .Where(op => op.HospitalId == hospitalId)
            .Include(op => op.Hospital)
            .ToListAsync();
    }

    public async Task<IEnumerable<Operation>> GetByClinicIdAsync(int clinicId)
    {
        return await _dbSet
            .Where(op => op.ClinicId == clinicId)
            .Include(op => op.Clinic)
            .ToListAsync();
    }

    public async Task<IEnumerable<Operation>> GetFatalOperationsAsync()
    {
        return await _dbSet
            .Where(op => op.IsFatal)
            .Include(op => op.Patient)
            .Include(op => op.Doctor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Operation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(op => op.Date >= startDate && op.Date <= endDate)
            .Include(op => op.Patient)
            .Include(op => op.Doctor)
            .ToListAsync();
    }
    
    // ✅ НОВА РЕАЛІЗАЦІЯ
    public async Task<IEnumerable<Operation>> GetAllWithAssociationsAsync()
    {
        return await _context.Operations
            .Include(op => op.Patient)
            .Include(op => op.Doctor)
            .Include(op => op.Hospital)
            .Include(op => op.Clinic)
            .AsNoTracking()
            .ToListAsync();
    }
}
