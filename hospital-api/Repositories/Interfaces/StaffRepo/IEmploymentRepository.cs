using hospital_api.Models.StaffAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IEmploymentRepository : IRepository<Employment>
{
    Task<IEnumerable<Employment>> GetEmploymentsByStaffIdAsync(int staffId);
    
    Task<IEnumerable<Employment>> GetEmploymentsByHospitalIdAsync(int hospitalId);
    
    Task<IEnumerable<Employment>> GetEmploymentsByClinicIdAsync(int clinicId);
}