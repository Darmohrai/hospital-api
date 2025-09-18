using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class SupportStaffService : ISupportStaffService
{
    private readonly ISupportStaffRepository _supportStaffRepository;

    public SupportStaffService(ISupportStaffRepository supportStaffRepository)
    {
        _supportStaffRepository = supportStaffRepository;
    }

    public async Task<IEnumerable<SupportStaff>> GetAllSupportStaffAsync()
    {
        return await _supportStaffRepository.GetAllAsync();
    }

    public async Task<SupportStaff?> GetSupportStaffByIdAsync(int id)
    {
        return await _supportStaffRepository.GetByIdAsync(id);
    }

    public async Task AddSupportStaffAsync(SupportStaff staff)
    {
        if (string.IsNullOrWhiteSpace(staff.FullName))
        {
            throw new ArgumentException("Support staff full name is required.");
        }

        await _supportStaffRepository.AddAsync(staff);
    }

    public async Task UpdateSupportStaffAsync(SupportStaff staff)
    {
        var existingStaff = await _supportStaffRepository.GetByIdAsync(staff.Id);
        if (existingStaff == null)
        {
            throw new InvalidOperationException("Support staff not found.");
        }

        await _supportStaffRepository.UpdateAsync(staff);
    }

    public async Task DeleteSupportStaffAsync(int id)
    {
        await _supportStaffRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<SupportStaff>> GetByRoleAsync(SupportRole role)
    {
        return await _supportStaffRepository.GetByRoleAsync(role);
    }

    public async Task<IEnumerable<SupportStaff>> GetByClinicIdAndRoleAsync(int clinicId, SupportRole role)
    {
        return await _supportStaffRepository.GetByClinicIdAndRoleAsync(clinicId, role);
    }

    public async Task<IEnumerable<SupportStaff>> GetByHospitalIdAndRoleAsync(int hospitalId, SupportRole role)
    {
        return await _supportStaffRepository.GetByHospitalIdAndRoleAsync(hospitalId, role);
    }

    public async Task<string> GetSupportStaffProfileSummaryAsync(int staffId)
    {
        var staff = await _supportStaffRepository.GetByIdAsync(staffId);

        if (staff == null)
        {
            return "Support staff not found.";
        }

        return $"Профіль співробітника: {staff.FullName}\n" +
               $"Роль: {staff.Role}\n" +
               $"Клініка ID: {staff.ClinicId}\n" +
               $"Лікарня ID: {staff.HospitalId}";
    }
}
