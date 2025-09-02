using hospital_api.Data;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty)
    {
        return await _dbSet.Where(d => d.Specialty == specialty).ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetByDegreeAsync(AcademicDegree degree)
    {
        return await _dbSet.Where(d => d.AcademicDegree == degree).ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetByTitleAsync(AcademicTitle title)
    {
        return await _dbSet.Where(d => d.AcademicTitle == title).ToListAsync();
    }
}