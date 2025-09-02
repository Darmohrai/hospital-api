using hospital_api.Models.ClinicAggregate;

namespace hospital_api.Repositories.Interfaces;

public interface IClinicRepository : IRepository<Clinic>
{
    // Додатковий метод специфічний для Clinic
    Task<bool> ExistsAsync(int id);
}