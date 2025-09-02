using hospital_api.Models.StaffAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IStaffRepository : IRepository<Staff>
{
    Task<IEnumerable<Staff>> GetByClinicIdAsync(int clinicId);
    Task<IEnumerable<Staff>> GetByHospitalIdAsync(int hospitalId);
}
