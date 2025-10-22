using System.Linq.Expressions;
using hospital_api.Data;
using hospital_api.Models.Tracking;
using hospital_api.Repositories.Interfaces.Tracking;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.Tracking;

public class ClinicDoctorAssignmentRepository : IClinicDoctorAssignmentRepository
{
    protected readonly ApplicationDbContext _context;

    public ClinicDoctorAssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAssignmentAsync(ClinicDoctorAssignment assignment)
    {
        await _context.ClinicDoctorAssignments.AddAsync(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAssignmentAsync(int patientId, int doctorId, int clinicId)
    {
        var assignment = await _context.ClinicDoctorAssignments
            .FirstOrDefaultAsync(a => a.PatientId == patientId && 
                                      a.DoctorId == doctorId && 
                                      a.ClinicId == clinicId);
        
        if (assignment != null)
        {
            _context.ClinicDoctorAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForPatientAsync(int patientId)
    {
        return await _context.ClinicDoctorAssignments
            .Where(a => a.PatientId == patientId)
            .Include(a => a.Doctor)
            .Include(a => a.Clinic)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClinicDoctorAssignment>> GetAssignmentsForDoctorAsync(int doctorId)
    {
        return await _context.ClinicDoctorAssignments
            .Where(a => a.DoctorId == doctorId)
            .Include(a => a.Patient)
            .Include(a => a.Clinic)
            .ToListAsync();
    }

    // --- Реалізація методів IRepository, які мають сенс ---

    public async Task<IEnumerable<ClinicDoctorAssignment>> GetAllAsync()
    {
        return await _context.ClinicDoctorAssignments.AsNoTracking().ToListAsync();
    }

    public IQueryable<ClinicDoctorAssignment> GetAll()
    {
        throw new NotSupportedException("This entity has a composite key. Use 'GetAllAsync'.");
    }

    public async Task<IEnumerable<ClinicDoctorAssignment>> FindByConditionAsync(Expression<Func<ClinicDoctorAssignment, bool>> expression)
    {
        return await _context.ClinicDoctorAssignments.Where(expression).ToListAsync();
    }
    
    // --- Методи IRepository, які не мають сенсу для цієї моделі ---

    public Task<ClinicDoctorAssignment?> GetByIdAsync(int id)
    {
        // Цей метод не можна реалізувати, оскільки немає єдиного Id
        throw new NotSupportedException("This entity has a composite key. Use specific 'Get' methods.");
    }

    public Task AddAsync(ClinicDoctorAssignment entity)
    {
        return AddAssignmentAsync(entity);
    }

    public Task UpdateAsync(ClinicDoctorAssignment entity)
    {
        // Оновлення M:M зв'язку - це зазвичай видалення + додавання,
        // але якщо сама сутність зв'язку має поля, то:
        _context.ClinicDoctorAssignments.Update(entity);
        return _context.SaveChangesAsync();
    }

    public Task DeleteAsync(int id)
    {
        // Цей метод не можна реалізувати
        throw new NotSupportedException("This entity has a composite key. Use 'RemoveAssignmentAsync'.");
    }
}