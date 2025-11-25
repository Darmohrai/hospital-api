using hospital_api.Data;
using hospital_api.Models.StaffAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Repositories.Implementations.StaffRepo;

public class StaffRepository : GenericRepository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SupportStaff>> GetSupportStaffByRoleAsync(SupportRole role)
    {
        return await _context.Staffs
            .OfType<SupportStaff>()
            .Where(s => s.Role == role)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportStaff>> GetSupportStaffByClinicAsync(int clinicId, SupportRole? role = null)
    {
        // Починаємо запит з працевлаштувань
        var query = _context.Employments
            .Where(e => e.ClinicId == clinicId)
            .Select(e => e.Staff)
            .OfType<SupportStaff>();

        if (role.HasValue)
        {
            query = query.Where(s => s.Role == role.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<SupportStaff>> GetSupportStaffByHospitalAsync(int hospitalId,
        SupportRole? role = null)
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

    public async Task<int> GetExtendedVacationDaysForDoctor(int doctorId)
    {
        var doctor = await _context.Staffs
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        switch (doctor)
        {
            case Neurologist n:
                return n.ExtendedVacationDays;
            case Ophthalmologist o:
                return o.ExtendedVacationDays;
            case Radiologist r:
                return r.ExtendedVacationDays;
            default:
                return 0;
        }
    }
    
    public async Task<float> GetHazardPayCoefficientForDoctor(int doctorId)
    {
        var doctor = await _context.Staffs
            .FirstOrDefaultAsync(d => d.Id == doctorId);

        switch (doctor)
        {
            case Radiologist r:
                return r.HazardPayCoefficient;
            default:
                return 0;
        }
    }
}