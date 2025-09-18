using hospital_api.Models.StaffAggregate;

namespace hospital_api.Services.Interfaces.StaffServices;

public interface IStaffService
{
    // Отримати весь персонал
    Task<IEnumerable<Staff>> GetAllStaffAsync();

    // Отримати персонал за ID
    Task<Staff?> GetStaffByIdAsync(int id);

    // Додати новий персонал
    Task AddStaffAsync(Staff staff);

    // Оновити дані персоналу
    Task UpdateStaffAsync(Staff staff);

    // Видалити персонал
    Task DeleteStaffAsync(int id);

    // Отримати персонал за ID клініки
    Task<IEnumerable<Staff>> GetStaffByClinicIdAsync(int clinicId);

    // Отримати персонал за ID лікарні
    Task<IEnumerable<Staff>> GetStaffByHospitalIdAsync(int hospitalId);

    // Отримати персонал з великим досвідом роботи
    Task<IEnumerable<Staff>> GetExperiencedStaffAsync(int minExperienceYears);

    // Застосувати логіку до даних про персонал
    // Наприклад, розрахувати річний бонус на основі досвіду
    Task<decimal> CalculateAnnualBonusAsync(int staffId);
}