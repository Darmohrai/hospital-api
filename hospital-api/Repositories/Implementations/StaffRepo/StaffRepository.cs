using hospital_api.Data;
using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class StaffRepository : GenericRepository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<SupportStaff>> GetSupportStaffByRoleAsync(SupportRole role)
    {
        return await _context.Staffs
            .OfType<SupportStaff>() // 1. Фільтруємо за типом
            .Where(s => s.Role == role) // 2. Фільтруємо за роллю
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportStaff>> GetSupportStaffByClinicAsync(int clinicId, SupportRole? role = null)
    {
        // Починаємо запит з працевлаштувань
        var query = _context.Employments
            .Where(e => e.ClinicId == clinicId)
            .Select(e => e.Staff) // Вибираємо пов'язаних співробітників
            .OfType<SupportStaff>(); // Із них вибираємо тільки SupportStaff

        if (role.HasValue)
        {
            query = query.Where(s => s.Role == role.Value); // Якщо роль вказана, додаємо фільтр
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<SupportStaff>> GetSupportStaffByHospitalAsync(int hospitalId, SupportRole? role = null)
    {
        var query = _context.Employments
            .Where(e => e.HospitalId == hospitalId)
            .Select(e => e.Staff)
            .OfType<SupportStaff>();

        if (role.HasValue)
        {
            query = query.Where(s => s.Role == role.Value);
        }

        return await query.ToListAsync();
    }
}