using hospital_api.Models.StaffAggregate.DoctorAggregate;
using hospital_api.Repositories.Interfaces.StaffRepo;
using hospital_api.Services.Interfaces.StaffServices;

namespace hospital_api.Services.Implementations.StaffServices;

public class CardiologistService : ICardiologistService
{
    private readonly ICardiologistRepository _cardiologistRepository;

    public CardiologistService(ICardiologistRepository cardiologistRepository)
    {
        _cardiologistRepository = cardiologistRepository;
    }

    public async Task<IEnumerable<Cardiologist>> GetAllCardiologistsAsync()
    {
        return await _cardiologistRepository.GetAllAsync();
    }

    public async Task<Cardiologist?> GetCardiologistByIdAsync(int id)
    {
        return await _cardiologistRepository.GetByIdAsync(id);
    }

    public async Task AddCardiologistAsync(Cardiologist cardiologist)
    {
        // Бізнес-логіка перед додаванням, наприклад, валідація
        if (string.IsNullOrWhiteSpace(cardiologist.FullName))
        {
            throw new ArgumentException("Cardiologist's full name is required.");
        }
        await _cardiologistRepository.AddAsync(cardiologist);
    }

    public async Task UpdateCardiologistAsync(Cardiologist cardiologist)
    {
        var existingCardiologist = await _cardiologistRepository.GetByIdAsync(cardiologist.Id);
        if (existingCardiologist == null)
        {
            throw new InvalidOperationException("Cardiologist not found.");
        }
        await _cardiologistRepository.UpdateAsync(cardiologist);
    }

    public async Task DeleteCardiologistAsync(int id)
    {
        await _cardiologistRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Cardiologist>> GetTopSurgeonsByOperationsAsync(int minOperations)
    {
        // Використовуємо метод репозиторію для фільтрації
        return await _cardiologistRepository.GetByOperationCountAsync(minOperations);
    }

    public async Task<IEnumerable<Cardiologist>> GetCardiologistsWithFatalOperationsAsync()
    {
        // Використовуємо метод репозиторію для фільтрації
        return await _cardiologistRepository.GetByFatalOperationCountAsync(1);
    }

    public async Task<string> GetCardiologistProfileSummaryAsync(int cardiologistId)
    {
        // Використовуємо метод репозиторію, який завантажує операції
        var cardiologist = await _cardiologistRepository.GetByIdAsync(cardiologistId);
        
        if (cardiologist == null)
        {
            return "Cardiologist not found.";
        }
        
        // Тут відбувається бізнес-логіка, наприклад, формування текстового звіту
        return $"Профіль лікаря: {cardiologist.FullName}\n" +
               $"Спеціалізація: {cardiologist.Specialty}\n" +
               $"Кількість операцій: {cardiologist.OperationCount}\n" +
               $"Кількість летальних операцій: {cardiologist.FatalOperationCount}";
    }
}