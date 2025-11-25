using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;

namespace hospital_api.Services.Implementations.StaffServices;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IEmploymentRepository _employmentRepository;

    public StaffService(IStaffRepository staffRepository, IEmploymentRepository employmentRepository)
    {
        _staffRepository = staffRepository;
        _employmentRepository = employmentRepository;
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _staffRepository.GetAllAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await _staffRepository.GetByIdAsync(id);
    }

    public async Task<ServiceResponse<Staff>> CreateAsync(Staff staff)
    {
        if (staff.WorkExperienceYears < 0)
        {
            return ServiceResponse<Staff>.Fail("Work experience cannot be negative.");
        }

        await _staffRepository.AddAsync(staff);
        return ServiceResponse<Staff>.Success(staff);
    }

    public async Task<ServiceResponse<Staff>> UpdateAsync(Staff staff)
    {
        var existingStaff = await _staffRepository.GetByIdAsync(staff.Id);
        if (existingStaff == null)
        {
            return ServiceResponse<Staff>.Fail("Staff member not found.");
        }

        await _staffRepository.UpdateAsync(staff);
        return ServiceResponse<Staff>.Success(staff);
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(int id)
    {
        var existingStaff = await _staffRepository.GetByIdAsync(id);
        if (existingStaff == null)
        {
            return ServiceResponse<bool>.Fail("Staff member not found.");
        }

        await _staffRepository.DeleteAsync(id);
        return ServiceResponse<bool>.Success(true);
    }

    public async Task<IEnumerable<Staff>> GetByClinicAsync(int clinicId)
    {
        var employments = await _employmentRepository.GetEmploymentsByClinicIdAsync(clinicId);
        return employments.Select(e => e.Staff);
    }

    public async Task<IEnumerable<Staff>> GetByHospitalAsync(int hospitalId)
    {
        var employments = await _employmentRepository.GetEmploymentsByHospitalIdAsync(hospitalId);
        return employments.Select(e => e.Staff);
    }

    public async Task<IEnumerable<Staff>> GetExperiencedStaffAsync(int minExperienceYears)
    {
        return await _staffRepository.GetAll()
            .Where(s => s.WorkExperienceYears >= minExperienceYears)
            .ToListAsync();
    }

    public async Task<ServiceResponse<decimal>> CalculateAnnualBonusAsync(int staffId)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            return ServiceResponse<decimal>.Fail("Staff member not found.");
        }

        decimal baseSalary = 50000m;
        decimal bonusRate = 0.01m;

        if (staff.WorkExperienceYears > 10)
        {
            bonusRate = 0.05m;
        }
        else if (staff.WorkExperienceYears > 5)
        {
            bonusRate = 0.03m;
        }

        var bonus = baseSalary * bonusRate;
        return ServiceResponse<decimal>.Success(bonus);
    }
}