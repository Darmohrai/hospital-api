using hospital_api.Models.OperationsAggregate;

namespace hospital_api.Services.Interfaces;

public interface IOperationService
{
    // CRUD
    Task<Operation?> GetByIdAsync(int id);
    Task<IEnumerable<Operation>> GetAllAsync();
    Task AddAsync(Operation operation);
    Task UpdateAsync(Operation operation);
    Task DeleteAsync(int id);

    // Специфічні методи
    Task<IEnumerable<Operation>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Operation>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Operation>> GetByHospitalIdAsync(int hospitalId);
    Task<IEnumerable<Operation>> GetByClinicIdAsync(int clinicId);
    Task<IEnumerable<Operation>> GetFatalOperationsAsync();
    Task<IEnumerable<Operation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
