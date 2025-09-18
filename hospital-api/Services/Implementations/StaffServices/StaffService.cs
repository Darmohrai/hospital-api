using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<Staff>> GetAllStaffAsync()
    {
        return await _staffRepository.GetAllAsync();
    }

    public async Task<Staff?> GetStaffByIdAsync(int id)
    {
        return await _staffRepository.GetByIdAsync(id);
    }

    public async Task AddStaffAsync(Staff staff)
    {
        // Приклад бізнес-логіки: перевірка умов перед додаванням
        if (staff.WorkExperienceYears < 0)
        {
            throw new ArgumentException("Work experience cannot be negative.");
        }
        await _staffRepository.AddAsync(staff);
    }

    public async Task UpdateStaffAsync(Staff staff)
    {
        // Приклад бізнес-логіки: перевірка умов перед оновленням
        var existingStaff = await _staffRepository.GetByIdAsync(staff.Id);
        if (existingStaff == null)
        {
            throw new InvalidOperationException("Staff member not found.");
        }
        // Тут може бути інша логіка, наприклад, валідація даних
        await _staffRepository.UpdateAsync(staff);
    }

    public async Task DeleteStaffAsync(int id)
    {
        await _staffRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Staff>> GetStaffByClinicIdAsync(int clinicId)
    {
        return await _staffRepository.GetByClinicIdAsync(clinicId);
    }

    public async Task<IEnumerable<Staff>> GetStaffByHospitalIdAsync(int hospitalId)
    {
        return await _staffRepository.GetByHospitalIdAsync(hospitalId);
    }

    // Приклад методу з бізнес-логікою, що не належить репозиторію
    public async Task<IEnumerable<Staff>> GetExperiencedStaffAsync(int minExperienceYears)
    {
        var allStaff = await _staffRepository.GetAllAsync();
        return allStaff.Where(s => s.WorkExperienceYears >= minExperienceYears);
    }
    
    public async Task<decimal> CalculateAnnualBonusAsync(int staffId)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null)
        {
            throw new InvalidOperationException("Staff member not found.");
        }
        
        // Тут застосовується унікальна бізнес-логіка, яка залежить від моделі
        decimal bonusRate = 0;
        if (staff.WorkExperienceYears > 10)
        {
            bonusRate = 0.05m;
        }
        else if (staff.WorkExperienceYears > 5)
        {
            bonusRate = 0.03m;
        }
        else
        {
            bonusRate = 0.01m;
        }
        
        // У цьому прикладі припустимо, що річний дохід розраховується якось інакше
        // і цей бонус залежить від нього. Для простоти, візьмемо умовну суму.
        return 50000 * bonusRate;
    }
}