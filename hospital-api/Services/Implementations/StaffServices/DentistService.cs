using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class DentistService : IDentistService
{
    private readonly IDentistRepository _dentistRepository;

    public DentistService(IDentistRepository dentistRepository)
    {
        _dentistRepository = dentistRepository;
    }

    public async Task<IEnumerable<Dentist>> GetAllDentistsAsync()
    {
        return await _dentistRepository.GetAllAsync();
    }

    public async Task<Dentist?> GetDentistByIdAsync(int id)
    {
        return await _dentistRepository.GetByIdAsync(id);
    }

    public async Task AddDentistAsync(Dentist dentist)
    {
        // Приклад бізнес-валідації перед додаванням
        if (dentist.HazardPayCoefficient < 0)
        {
            throw new ArgumentException("Hazard pay coefficient cannot be negative.");
        }
        await _dentistRepository.AddAsync(dentist);
    }

    public async Task UpdateDentistAsync(Dentist dentist)
    {
        var existingDentist = await _dentistRepository.GetByIdAsync(dentist.Id);
        if (existingDentist == null)
        {
            throw new InvalidOperationException("Dentist not found.");
        }
        await _dentistRepository.UpdateAsync(dentist);
    }

    public async Task DeleteDentistAsync(int id)
    {
        await _dentistRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Dentist>> GetTopPerformingDentistsAsync(int minOperationCount)
    {
        // Використовуємо спеціалізований метод репозиторію
        return await _dentistRepository.GetByOperationCountAsync(minOperationCount);
    }

    public async Task<IEnumerable<Dentist>> GetDentistsWithHighHazardPayAsync(float minCoefficient)
    {
        // Використовуємо спеціалізований метод репозиторію
        return await _dentistRepository.GetByHazardPayCoefficientAsync(minCoefficient);
    }
    
    public async Task<string> GetDentistSummaryAsync(int dentistId)
    {
        // Завантажуємо дантиста разом з операціями
        var dentist = await _dentistRepository.GetByIdAsync(dentistId);
        
        if (dentist == null)
        {
            return "Dentist not found.";
        }
        
        // Формуємо текстовий звіт, що є частиною бізнес-логіки
        return $"Профіль: {dentist.FullName}\n" +
               $"Спеціальність: {dentist.Specialty}\n" +
               $"Кількість операцій: {dentist.OperationCount}\n" +
               $"Летальних операцій: {dentist.FatalOperationCount}\n" +
               $"Коефіцієнт шкідливості: {dentist.HazardPayCoefficient}";
    }
}