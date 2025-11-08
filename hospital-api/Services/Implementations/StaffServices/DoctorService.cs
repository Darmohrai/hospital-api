using hospital_api.Models.StaffAggregate;
using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace hospital_api.Services.Implementations.StaffServices;

public class DoctorService : IDoctorService
{
    private readonly IStaffRepository _staffRepository;

    // ✅ Конструктор тепер чистий і має лише одну залежність!
    public DoctorService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _staffRepository.GetAll().OfType<Doctor>().ToListAsync();
    }
    
    

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff as Doctor;
    }

    public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty)
    {
        return await _staffRepository.GetAll()
            .OfType<Doctor>()
            .Where(d => d.Specialty.Equals(specialty, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetByDegreeAsync(AcademicDegree degree)
    {
        return await _staffRepository.GetAll()
            .OfType<Doctor>()
            .Where(d => d.AcademicDegree == degree)
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetByTitleAsync(AcademicTitle title)
    {
        return await _staffRepository.GetAll()
            .OfType<Doctor>()
            .Where(d => d.AcademicTitle == title)
            .ToListAsync();
    }

    // --- Специфічна бізнес-логіка ---

    // ✅ Ефективний метод, що робить ОДИН запит до БД
    public async Task<IEnumerable<Doctor>> GetWithHazardPayAsync()
    {
        return await _staffRepository.GetAll()
            .Where(s => s is Dentist || s is Radiologist) // Фільтруємо на рівні базового класу
            .OfType<Doctor>() // Конвертуємо результат у лікарів
            .ToListAsync();
    }

    // ✅ Ефективний метод, що робить ОДИН запит до БД
    public async Task<IEnumerable<Doctor>> GetWithExtendedVacationAsync()
    {
        return await _staffRepository.GetAll()
            .Where(s => s is Neurologist || s is Ophthalmologist || s is Radiologist)
            .OfType<Doctor>()
            .ToListAsync();
    }

    // ✅ Коректна реалізація з жадібним завантаженням (Eager Loading)
    public async Task<IEnumerable<Doctor>> GetProfessorsWithMultipleAssignmentsAsync()
    {
        return await _staffRepository.GetAll()
            .OfType<Doctor>()
            .Include(d => d.Assignments) // Підвантажуємо пов'язані дані
            .Where(d => d.AcademicTitle == AcademicTitle.Professor && d.Assignments.Count > 1)
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync(string? specialty, AcademicDegree? degree, AcademicTitle? title)
    {
        // Починаємо запит як IQueryable<Doctor>
        IQueryable<Doctor> query = _staffRepository.GetAll()
            .OfType<Doctor>()
            .Include(d => d.Employments); 

        // --- ✅ ФІНАЛЬНА ЛОГІКА ФІЛЬТРАЦІЇ ---
    
        if (!string.IsNullOrEmpty(specialty))
        {
            // ✅ ВИПРАВЛЕННЯ:
            // Використовуємо .ToUpper() - це перекладається в SQL (UPPER(...))
            // Додаємо перевірку d.Specialty != null для уникнення помилок з null
            string specialtyUpper = specialty.ToUpper();
            query = query.Where(d => d.Specialty != null && d.Specialty.ToUpper() == specialtyUpper);
        }

        if (degree.HasValue)
        {
            // Просто застосовуємо .Where
            query = query.Where(d => d.AcademicDegree == degree.Value);
        }

        if (title.HasValue)
        {
            // Просто застосовуємо .Where
            query = query.Where(d => d.AcademicTitle == title.Value);
        }
        // --- Кінець логіки фільтрації ---

        return await query.ToListAsync();
    }
}