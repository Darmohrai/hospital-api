using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IDoctorService
{
    // CRUD
    Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
    Task<Doctor?> GetDoctorByIdAsync(int id);
    Task AddDoctorAsync(Doctor doctor);
    Task UpdateDoctorAsync(Doctor doctor);
    Task DeleteDoctorAsync(int id);

    // Фільтрація
    Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(string specialty);
    Task<IEnumerable<Doctor>> GetDoctorsByDegreeAsync(AcademicDegree degree);
    Task<IEnumerable<Doctor>> GetDoctorsByTitleAsync(AcademicTitle title);
}