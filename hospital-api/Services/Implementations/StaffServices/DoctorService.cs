using hospital_api.Models.StaffAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDentistRepository _dentistRepository;
    private readonly IRadiologistRepository _radiologistRepository;
    private readonly INeurologistRepository _neurologistRepository;
    private readonly IOphthalmologistRepository _ophthalmologistRepository;
    private readonly ISurgeonRepository _surgeonRepository;
    private readonly IGynecologistRepository _gynecologistRepository;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IDentistRepository dentistRepository,
        IRadiologistRepository radiologistRepository,
        INeurologistRepository neurologistRepository,
        IOphthalmologistRepository ophthalmologistRepository,
        ISurgeonRepository surgeonRepository,
        IGynecologistRepository gynecologistRepository)
    {
        _doctorRepository = doctorRepository;
        _dentistRepository = dentistRepository;
        _radiologistRepository = radiologistRepository;
        _neurologistRepository = neurologistRepository;
        _ophthalmologistRepository = ophthalmologistRepository;
        _surgeonRepository = surgeonRepository;
        _gynecologistRepository = gynecologistRepository;
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
    {
        return await _doctorRepository.GetAllAsync();
    }
    
    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        return await _doctorRepository.GetByIdAsync(id);
    }
    
    public async Task AddDoctorAsync(Doctor doctor)
    {
        await _doctorRepository.AddAsync(doctor);
    }
    
    public async Task UpdateDoctorAsync(Doctor doctor)
    {
        await _doctorRepository.UpdateAsync(doctor);
    }
    
    public async Task DeleteDoctorAsync(int id)
    {
        await _doctorRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(string specialty)
    {
        return await _doctorRepository.GetBySpecialtyAsync(specialty);
    }
    
    public async Task<IEnumerable<Doctor>> GetDoctorsByDegreeAsync(AcademicDegree degree)
    {
        return await _doctorRepository.GetByDegreeAsync(degree);
    }
    
    public async Task<IEnumerable<Doctor>> GetDoctorsByTitleAsync(AcademicTitle title)
    {
        return await _doctorRepository.GetByTitleAsync(title);
    }

    // Приклад бізнес-логіки: об'єднання даних з різних репозиторіїв
    public async Task<IEnumerable<Doctor>> GetDoctorsWithHazardPayAsync()
    {
        var dentists = await _dentistRepository.GetByHazardPayCoefficientAsync(0.1f);
        var radiologists = await _radiologistRepository.GetByHazardPayCoefficientAsync(0.1f);

        // Виправлення: Явно вказуємо тип Doctor для методу Union
        return dentists.Union<Doctor>(radiologists);
    }

    public async Task<IEnumerable<Doctor>> GetDoctorsWithExtendedVacationAsync()
    {
        var neurologists = await _neurologistRepository.GetByExtendedVacationDaysAsync(14);
        var ophthalmologists = await _ophthalmologistRepository.GetByExtendedVacationDaysAsync(14);
        var radiologists = await _radiologistRepository.GetByExtendedVacationDaysAsync(14);

        // Виправлення: Явно вказуємо тип Doctor для кожного об'єднання
        return neurologists
            .Union<Doctor>(ophthalmologists)
            .Union<Doctor>(radiologists);
    }
    
    public async Task<IEnumerable<Doctor>> GetSurgeonsWithFatalOperationsAsync()
    {
        var surgeons = await _surgeonRepository.GetAllWithOperationsAsync();
        return surgeons.Where(s => s.FatalOperationCount > 0);
    }
    
    public async Task<IEnumerable<Doctor>> GetProfessorsWithMultipleAssignmentsAsync()
    {
        // Припустимо, що DoctorRepository має метод, що завантажує Assignments
        var doctors = await _doctorRepository.GetAllAsync();
        // ВАЖЛИВО: Для коректної роботи потрібно завантажити DoctorAssignments
        // Репозиторій повинен мати метод GetByTitleWithAssignmentsAsync
        return doctors
            .Where(d => d.AcademicTitle == AcademicTitle.Professor && d.Assignments.Count > 1);
    }
}