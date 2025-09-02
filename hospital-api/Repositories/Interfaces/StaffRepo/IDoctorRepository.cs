using hospital_api.Models.StaffAggregate;

namespace hospital_api.Repositories.Interfaces.StaffRepo;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty);
    Task<IEnumerable<Doctor>> GetByDegreeAsync(AcademicDegree degree);
    Task<IEnumerable<Doctor>> GetByTitleAsync(AcademicTitle title);
}