using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IDoctorService
{
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<Doctor?> GetByIdAsync(int id);
    Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty);
    Task<IEnumerable<Doctor>> GetByDegreeAsync(AcademicDegree degree);
    Task<IEnumerable<Doctor>> GetByTitleAsync(AcademicTitle title);
        
    // --- Специфічна бізнес-логіка ---
    Task<IEnumerable<Doctor>> GetWithHazardPayAsync();
    Task<IEnumerable<Doctor>> GetWithExtendedVacationAsync();
    
    Task<IEnumerable<Doctor>> GetAllAsync(string? specialty, AcademicDegree? degree, AcademicTitle? title);
}